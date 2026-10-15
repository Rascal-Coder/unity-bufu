using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bufu.Rebuild.UI;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

/// <summary>Reconstructs original native particle modules; never approximates their curves or simulation.</summary>
public static class NativeParticleBuilder {
    const string Target="Assets/Resources/UI/NativeParticles";
    static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
    static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
    static Dictionary<string,Dictionary<string,object>> materialSources;

    /// <summary>Records the actual editor-native JSON schema without building a player or modifying source assets.</summary>
    public static void DumpSchema() {
        const string folder="Build/HonorSupportEvidence/Schema";
        Directory.CreateDirectory(folder);
        var source=Component((Dictionary<string,object>)ReadSource("Popup/goalReachPanel").Arr("nodes")[15],"ParticleSystem");
        var go=new GameObject("Native Particle Schema");go.SetActive(false);
        try {
            var particle=go.AddComponent<ParticleSystem>();particle.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            File.WriteAllText(folder+"/before.json",EditorJsonUtility.ToJson(particle,true));
            var direct=new Dictionary<string,object>(source);direct.Remove("t");direct.Remove("UVModule");
            File.WriteAllText(folder+"/source.json",NativeParticleJson.Stringify(direct));
            EditorJsonUtility.FromJsonOverwrite(NativeParticleJson.Stringify(direct),particle);
            File.WriteAllText(folder+"/after-flat.json",EditorJsonUtility.ToJson(particle,true));
            float flatDuration=particle.main.duration;
            EditorJsonUtility.FromJsonOverwrite(NativeParticleJson.Stringify(new Dictionary<string,object>{{"ParticleSystem",direct}}),particle);
            File.WriteAllText(folder+"/after-wrapped.json",EditorJsonUtility.ToJson(particle,true));
            var merged=MergeNativeSchema(MiniJson.Parse(File.ReadAllText(folder+"/before.json")),direct,particle.GetType().Name);
            File.WriteAllText(folder+"/source-schema-merged.json",NativeParticleJson.Stringify(merged));
            EditorJsonUtility.FromJsonOverwrite(NativeParticleJson.Stringify(merged),particle);
            File.WriteAllText(folder+"/after-schema-merged.json",EditorJsonUtility.ToJson(particle,true));
            var properties=new List<object>();var serialized=new SerializedObject(particle);var property=serialized.GetIterator();
            while(property.Next(true))properties.Add(new Dictionary<string,object>{{"path",property.propertyPath},{"type",property.type},{"kind",property.propertyType.ToString()}});
            File.WriteAllText(folder+"/properties.json",NativeParticleJson.Stringify(properties));
            Debug.Log("NATIVE_PARTICLE_SCHEMA flatDuration="+flatDuration+" schemaMergedDuration="+particle.main.duration+" path="+folder);
        } finally {Object.DestroyImmediate(go);}
    }

    [MenuItem("不服开一局/重建原版粒子")]
    public static void Build() {
        Directory.CreateDirectory(Target+"/Materials");Directory.CreateDirectory(Target+"/Sprites");
        materials.Clear();sprites.Clear();materialSources=ReadMaterials();
        AssetDatabase.Refresh();
        int count=0;
        foreach(string source in NativeParticles.Sources) {
            var data=ReadSource(source);
            var nodes=data.Arr("nodes");
            for(int index=0;index<nodes.Count;index++) {
                var node=(Dictionary<string,object>)nodes[index];
                var systemData=Component(node,"ParticleSystem");
                if(systemData==null)continue;
                var rendererData=Component(node,"ParticleSystemRenderer");
                if(rendererData==null)throw new InvalidOperationException(source+" node "+index+" has no original ParticleSystemRenderer data");
                var go=new GameObject(node.Str("n"));go.SetActive(false);
                try {
                    var system=go.AddComponent<ParticleSystem>();
                    system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                    Overwrite(system,systemData,source+"/"+index+"/ParticleSystem");
                    var renderer=go.GetComponent<ParticleSystemRenderer>();
                    Overwrite(renderer,rendererData,source+"/"+index+"/ParticleSystemRenderer");
                    // Coffee owns mesh baking and uGUI drawing; native camera rendering must remain disabled.
                    renderer.enabled=false;
                    var prefabPath="Assets/Resources/"+NativeParticles.ResourcePath(source,index)+".prefab";
                    PrefabUtility.SaveAsPrefabAsset(go,prefabPath);
                    count++;
                } finally {Object.DestroyImmediate(go);}
            }
        }
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        Verify();
        Debug.Log("NATIVE_PARTICLE_BUILD_OK count="+count);
    }

    [MenuItem("不服开一局/验证原版粒子")]
    public static void Verify() {
        if(materialSources==null)materialSources=ReadMaterials();
        int count=0;
        foreach(string source in NativeParticles.Sources) {
            var nodes=ReadSource(source).Arr("nodes");
            for(int index=0;index<nodes.Count;index++) {
                var node=(Dictionary<string,object>)nodes[index];var original=Component(node,"ParticleSystem");
                if(original==null)continue;
                string path="Assets/Resources/"+NativeParticles.ResourcePath(source,index)+".prefab";
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if(!prefab)throw new InvalidOperationException("Missing native particle prefab: "+path);
                var system=prefab.GetComponent<ParticleSystem>();var renderer=prefab.GetComponent<ParticleSystemRenderer>();
                if(!system||!renderer)throw new InvalidOperationException(path+" is not a native ParticleSystem");
                Near(system.main.duration,original.F("lengthInSec"),source+" duration");
                if(system.main.loop!=original.B("looping"))throw new InvalidOperationException(source+" loop did not roundtrip");
                if(system.textureSheetAnimation.enabled!=original.Obj("UVModule").B("enabled"))throw new InvalidOperationException(source+" UV module enable did not roundtrip");
                if(system.noise.enabled!=original.Obj("NoiseModule").B("enabled"))throw new InvalidOperationException(source+" Noise module enable did not roundtrip");
                // Verify every original serialized module, including all min/max curves, gradients, UV and noise values.
                var restored=SchemaBody((Dictionary<string,object>)MiniJson.Parse(EditorJsonUtility.ToJson(system)),system.GetType().Name);
                var references=new SerializedObject(system);references.Update();
                VerifyObjectReferences(references,original,"",source+"/"+index);
                foreach(var pair in original) {
                    if(pair.Key=="t")continue;
                    if(!restored.TryGetValue(pair.Key,out var actual))throw new InvalidOperationException(source+" unsupported particle field "+pair.Key);
                    Compare(pair.Value,actual,source+"/"+index+"/"+pair.Key);
                }
                var rdata=Component(node,"ParticleSystemRenderer");
                var expectedMaterials=rdata.Arr("m_Materials");
                if(expectedMaterials==null||renderer.sharedMaterials.Length!=expectedMaterials.Count)throw new InvalidOperationException(source+" original renderer material count differs");
                for(int m=0;m<expectedMaterials.Count;m++) {
                    if(expectedMaterials[m]==null) {
                        if(renderer.sharedMaterials[m])throw new InvalidOperationException(source+" original null material binding differs at "+m);
                        continue;
                    }
                    var key=((Dictionary<string,object>)expectedMaterials[m]).Str("material");
                    var expected=AssetDatabase.LoadAssetAtPath<Material>(Target+"/Materials/"+Safe(key)+".mat");
                    if(!expected||renderer.sharedMaterials[m]!=expected)throw new InvalidOperationException(source+" material binding differs: "+key);
                    if(expected.HasProperty("_MainTex")&&!expected.mainTexture)throw new InvalidOperationException(source+" native material texture missing: "+key);
                }
                if(renderer.enabled)throw new InvalidOperationException(source+" camera renderer was not disabled for Coffee UIParticle");
                count++;
            }
        }
        Debug.Log("NATIVE_PARTICLE_VERIFY_OK count="+count+" all original modules/curves/UV/Noise/materials preserved");
    }

    static void Overwrite(Object target,Dictionary<string,object> source,string context) {
        var converted=(Dictionary<string,object>)ConvertReferences(source,context);
        converted.Remove("t");
        // UnityPy typetree export omits serializedVersion metadata. Restore the current native
        // schema (including nested curve/keyframe versions) instead of invoking legacy migrations.
        var schema=MiniJson.Parse(EditorJsonUtility.ToJson(target));
        var complete=MergeNativeSchema(schema,converted,target.GetType().Name);
        EditorJsonUtility.FromJsonOverwrite(NativeParticleJson.Stringify(complete),target);
        // Engine JSON restores native module values but does not resolve persistent
        // object instance IDs in this Tuanjie version. Restore each original PPtr
        // through the engine's serialized object-reference property instead.
        var serialized=new SerializedObject(target);serialized.Update();
        BindObjectReferences(serialized,source,"",context);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    static void BindObjectReferences(SerializedObject target,object original,string path,string context) {
        if(original is Dictionary<string,object> source) {
            Object reference=null;
            if(source.TryGetValue("material",out var material)&&material is string materialKey)reference=GetMaterial(materialKey);
            else if(source.TryGetValue("sprite",out var sprite)&&sprite is string spriteKey)reference=GetSprite(spriteKey);
            if(reference) {
                var property=target.FindProperty(path);
                if(property==null||property.propertyType!=SerializedPropertyType.ObjectReference)
                    throw new InvalidOperationException("Original object reference has no native serialized property: "+context+"/"+path);
                property.objectReferenceValue=reference;return;
            }
            foreach(var pair in source)if(pair.Key!="t")BindObjectReferences(target,pair.Value,string.IsNullOrEmpty(path)?pair.Key:path+"."+pair.Key,context);
        } else if(original is List<object> list) {
            for(int i=0;i<list.Count;i++)BindObjectReferences(target,list[i],path+".Array.data["+i+"]",context);
        }
    }

    static void VerifyObjectReferences(SerializedObject target,object original,string path,string context) {
        if(original is Dictionary<string,object> source) {
            Object expected=null;
            if(source.TryGetValue("material",out var material)&&material is string materialKey)expected=GetMaterial(materialKey);
            else if(source.TryGetValue("sprite",out var sprite)&&sprite is string spriteKey)expected=GetSprite(spriteKey);
            if(expected) {
                var property=target.FindProperty(path);
                if(property==null||property.propertyType!=SerializedPropertyType.ObjectReference)
                    throw new InvalidOperationException("Original object reference has no native serialized property: "+context+"/"+path);
                if(property.objectReferenceValue!=expected)
                    throw new InvalidOperationException("Original object identity differs: "+context+"/"+path+" expected "+AssetDatabase.GetAssetPath(expected));
                return;
            }
            foreach(var pair in source)if(pair.Key!="t")VerifyObjectReferences(target,pair.Value,string.IsNullOrEmpty(path)?pair.Key:path+"."+pair.Key,context);
        } else if(original is List<object> list) {
            for(int i=0;i<list.Count;i++)VerifyObjectReferences(target,list[i],path+".Array.data["+i+"]",context);
        }
    }

    static Dictionary<string,object> SchemaBody(Dictionary<string,object> schema,string type) {
        return schema.TryGetValue(type,out var body)&&body is Dictionary<string,object> data?data:schema;
    }
    static object MergeNativeSchema(object schema,Dictionary<string,object> original,string type) {
        var root=(Dictionary<string,object>)schema;
        if(root.TryGetValue(type,out var wrapped)&&wrapped is Dictionary<string,object>) {
            var result=new Dictionary<string,object>(root);result[type]=MergeSchemaValue(wrapped,original);return result;
        }
        return MergeSchemaValue(schema,original);
    }
    static object MergeSchemaValue(object schema,object original) {
        if(original is Dictionary<string,object> source) {
            var defaults=schema as Dictionary<string,object>;
            var result=defaults==null?new Dictionary<string,object>():new Dictionary<string,object>(defaults);
            foreach(var pair in source){object before=null;defaults?.TryGetValue(pair.Key,out before);result[pair.Key]=MergeSchemaValue(before,pair.Value);}
            return result;
        }
        if(original is List<object> list) {
            var defaults=schema as List<object>;var result=new List<object>();
            for(int i=0;i<list.Count;i++)result.Add(MergeSchemaValue(defaults!=null&&defaults.Count>0?defaults[Math.Min(i,defaults.Count-1)]:null,list[i]));
            return result;
        }
        return original;
    }

    static object ConvertReferences(object value,string context) {
        if(value is Dictionary<string,object> dictionary) {
            if(dictionary.TryGetValue("material",out var material))return new Dictionary<string,object>{{"instanceID",GetMaterial((string)material).GetInstanceID()}};
            if(dictionary.TryGetValue("sprite",out var sprite)&&sprite is string spriteKey)return new Dictionary<string,object>{{"instanceID",GetSprite(spriteKey).GetInstanceID()}};
            if(dictionary.ContainsKey("node")||dictionary.ContainsKey("asset")||dictionary.ContainsKey("builtin")||dictionary.ContainsKey("prefab")||dictionary.ContainsKey("texture"))
                throw new NotSupportedException("Unresolved original particle object reference at "+context+": "+NativeParticleJson.Stringify(dictionary));
            var result=new Dictionary<string,object>();
            foreach(var pair in dictionary)if(pair.Key!="t")result[pair.Key]=ConvertReferences(pair.Value,context+"/"+pair.Key);
            return result;
        }
        if(value is List<object> list)return list.Select((entry,index)=>ConvertReferences(entry,context+"["+index+"]")).ToList();
        return value;
    }

    static Material GetMaterial(string key) {
        if(materials.TryGetValue(key,out var result))return result;
        if(!materialSources.TryGetValue(key,out var source))throw new InvalidOperationException("Original particle material metadata missing: "+key);
        // Coffee draws baked particle meshes through CanvasRenderer. The built-in
        // Standard particle shader expects the camera particle rendering path;
        // use uGUI's straight-alpha shader for these unlit sprite particles.
        bool canvasShader=source.Str("shader")=="Particles/Standard Unlit";
        var shader=Shader.Find(canvasShader?"UI/Default":source.Str("shader"));
        if(!shader)throw new InvalidOperationException("Original particle shader missing: "+source.Str("shader"));
        string path=Target+"/Materials/"+Safe(key)+".mat";
        result=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!result){result=new Material(shader);AssetDatabase.CreateAsset(result,path);}else result.shader=shader;
        result.name=source.Str("name",key);
        var floats=source.Obj("floats");if(floats!=null)foreach(var p in floats)if(result.HasProperty(p.Key))result.SetFloat(p.Key,Convert.ToSingle(p.Value));
        var colors=source.Obj("colors");if(colors!=null)foreach(var p in colors)if(result.HasProperty(p.Key)) {
            var c=(List<object>)p.Value;result.SetColor(p.Key,new Color(Convert.ToSingle(c[0]),Convert.ToSingle(c[1]),Convert.ToSingle(c[2]),Convert.ToSingle(c[3])));
        }
        var textures=source.Obj("textures");
        if(textures!=null)foreach(var p in textures) {
            var t=(Dictionary<string,object>)p.Value;
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/UI/Textures/"+t.Str("key")+".png");
            if(!texture)throw new InvalidOperationException("Original particle texture missing: "+t.Str("key"));
            if(result.HasProperty(p.Key)){result.SetTexture(p.Key,texture);result.SetTextureScale(p.Key,t.V2("scale"));result.SetTextureOffset(p.Key,t.V2("offset"));}
        }
        var keywords=source.Arr("keywords");
        result.shaderKeywords=canvasShader?Array.Empty<string>():keywords!=null?keywords.OfType<string>().ToArray():source.Str("keywords").Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
        EditorUtility.SetDirty(result);materials[key]=result;return result;
    }

    static Sprite GetSprite(string key) {
        if(sprites.TryGetValue(key,out var sprite))return sprite;
        string path=Target+"/Sprites/"+Safe(key)+".asset";
        sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(!sprite) {
            sprite=UiAssets.Sprite(key);
            if(!sprite)throw new InvalidOperationException("Original particle UV sprite missing: "+key);
            AssetDatabase.CreateAsset(sprite,path);
            if(!AssetDatabase.Contains(sprite.texture))AssetDatabase.AddObjectToAsset(sprite.texture,sprite);
        }
        sprites[key]=sprite;return sprite;
    }

    static Dictionary<string,Dictionary<string,object>> ReadMaterials() {
        var source=(Dictionary<string,object>)MiniJson.Parse(File.ReadAllText("Assets/Resources/UI/materials.json"));
        var result=new Dictionary<string,Dictionary<string,object>>();
        foreach(Dictionary<string,object> row in source.Arr("materials"))result[row.Str("key")]=row;
        return result;
    }
    static Dictionary<string,object> ReadSource(string resource)=>
        (Dictionary<string,object>)MiniJson.Parse(File.ReadAllText("Assets/Resources/UI/Prefabs/"+resource+".json"));
    static Dictionary<string,object> Component(Dictionary<string,object> node,string type)=>node.Arr("c").OfType<Dictionary<string,object>>().FirstOrDefault(value=>value.Str("t")==type);
    static string Safe(string text)=>string.Join("_",text.Split(Path.GetInvalidFileNameChars()));
    static void Near(double original,double restored,string context) {
        if(Math.Abs(original-restored)>1e-5*Math.Max(1,Math.Max(Math.Abs(original),Math.Abs(restored))))throw new InvalidOperationException(context+" original/native value differs: "+original+" / "+restored);
    }
    static void Compare(object original,object restored,string context) {
        if(original==null){if(restored!=null)throw new InvalidOperationException(context+" null differs");return;}
        if(original is Dictionary<string,object> a) {
            // Persistent Unity PPtrs serialize as fileID/guid, while transient
            // PPtrs serialize as instanceID. Verify their actual object identity
            // separately through SerializedObject, never compare those encodings.
            if((a.TryGetValue("sprite",out var sprite)&&sprite is string)||(a.TryGetValue("material",out var material)&&material is string))return;
            if(!(restored is Dictionary<string,object> b))throw new InvalidOperationException(context+" object differs");
            foreach(var p in a){if(!b.TryGetValue(p.Key,out var value))throw new InvalidOperationException(context+"/"+p.Key+" missing");Compare(p.Value,value,context+"/"+p.Key);}return;
        }
        if(original is List<object> x) {
            if(!(restored is List<object> y)||x.Count!=y.Count)throw new InvalidOperationException(context+" array differs");
            for(int i=0;i<x.Count;i++)Compare(x[i],y[i],context+"["+i+"]");return;
        }
        if(original is double||original is float||original is int||original is long) {Near(Convert.ToDouble(original),Convert.ToDouble(restored),context);return;}
        if(original is bool flag) {if(flag!=Convert.ToBoolean(restored))throw new InvalidOperationException(context+" bool differs");return;}
        if(!Equals(original,restored))throw new InvalidOperationException(context+" differs");
    }
}
