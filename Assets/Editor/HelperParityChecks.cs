using System;
using System.Collections.Generic;
using System.Reflection;
using Bufu.Rebuild;
using Bufu.Rebuild.UI;
using UnityEngine;
using UnityEngine.UI;

public static class HelperParityChecks {
    public static void Verify(Action<bool,string> check) {
        var model=new GameModel();var role=model.HelperConfig(10);
        check(Mathf.Abs(HelperSkills.DiceMoveDistance(role,1)-400)<.001f&&Mathf.Abs(HelperSkills.DiceMoveDistance(role,5)-480)<.001f&&Mathf.Abs(HelperSkills.DiceMoveDistance(role,70)-1360)<.001f,
            "helper parity: serialized dice cross travels 400/480/1360 original pixels at levels 1/5/70");
        check(HelperSkills.BasketballBounces(0)==0&&HelperSkills.BasketballBounces(1)==1&&HelperSkills.BasketballBounces(2)==2&&HelperSkills.BasketballBounces(3)==3,
            "helper parity: basketball awakening permits zero, one, two and three boundary rebounds");
        model.Data.worlds[1].talents[305]=2;
        check(Mathf.Abs(HelperSkills.DiceMoveWidth(model)-1.35f)<.001f,"helper parity: bomb width uses the saved dice-world powder talent even in the coin world");
        var prefab=(Dictionary<string,object>)MiniJson.Parse(Resources.Load<TextAsset>("UI/Prefabs/HelperSkills/dushen_DiceMove").text);
        var node=(Dictionary<string,object>)prefab.Arr("nodes")[0];var script=(Dictionary<string,object>)node.Arr("c")[0];
        check(script.F("moveSpeed")==600&&script.F("baseMoveDistance")==400&&Mathf.Abs(script.F("sourceScale")-.4f)<.001f,
            "helper parity: recovered serialized prefab confirms speed 600, base distance 400 and source scale 0.4");
        var scene=ReadScript("UI/Prefabs/Common/MainSceneVat","MainSceneVat");
        check(scene.F("spatialIndexCellSize")==120&&HelperSkills.IronCellSize==scene.F("spatialIndexCellSize")&&Mathf.Abs(scene.F("uiToWorldScale")-.01f)<.000001f,
            "helper parity: dense targeting uses the verified scene prefab's 120-pixel spatial cells");
        var randomState=UnityEngine.Random.state;
        var host=new GameObject("Helper parity checks",typeof(RectTransform));host.SetActive(false);
        try {
            var game=host.AddComponent<GameController>();game.enabled=false;
            typeof(GameController).GetProperty("Model").SetValue(game,model);
            game.Field=(RectTransform)host.transform;game.Field.sizeDelta=new Vector2(1120,1000);
            game.SmokePool=GameController.NewRect("Skill pool",game.Field);
            var basket=Shot(game,SkillShot.Kind.Basketball,0);basket.bounces=2;
            basket.Tick(.6f);basket.Tick(.5f);
            check(basket&&Mathf.Abs(((RectTransform)basket.transform).anchoredPosition.x-30)<.01f&&basket.bounces==1,
                "helper parity: basketball remains alive after 1100 traveled pixels and expires by rebound budget rather than fixed distance");
            var heart=Shot(game,SkillShot.Kind.Heart,500);heart.comeBack=true;heart.suckRadius=150;
            heart.transform.localEulerAngles=new Vector3(0,0,17);heart.SetMode(SkillShot.Kind.Heart);
            check(Mathf.Abs(Mathf.DeltaAngle(heart.transform.localEulerAngles.z,17))<.001f,
                "helper parity: fox projectiles preserve the authored angle instead of rotating with their travel direction");
            heart.Tick(.5f);
            var coinRect=GameController.NewRect("Charm target",game.Field);coinRect.anchoredPosition=new Vector2(500,100);
            var coin=coinRect.gameObject.AddComponent<CoinActor>();coin.Game=game;coin.World=1;coin.Quality=1;game.Coins.Add(coin);
            var queue=GameController.NewRect("Queued skill checks",game.SmokePool).gameObject.AddComponent<HelperTossQueue>();queue.Begin(game);
            check(queue.Enqueue(coin,8,1)&&!queue.Enqueue(coin,10,6)&&!coin.Busy,
                "helper parity: queued explosions reserve each coin once before the sixteen-per-frame launch stage");
            var hits=(HashSet<int>)typeof(SkillShot).GetField("seen",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(heart);hits.Add(123);
            Vector2 before=coinRect.anchoredPosition;heart.Tick(.25f);
            check(((RectTransform)heart.transform).anchoredPosition==new Vector2(500,0)&&coinRect.anchoredPosition!=before&&!coin.Busy,
                "helper parity: fox pauses at the far end and moves idle coins physically without throwing them");
            heart.Tick(.25f);
            check(hits.Count==0,"helper parity: fox clears outbound hits before its return trip");
            game.Coins.Clear();heart.Tick(.1f);
            check(Mathf.Abs(((RectTransform)heart.transform).anchoredPosition.x-400)<.01f,
                "helper parity: fox returns only after the original half-second pause");
            var frames=GameController.NewRect("Tentacle frames",game.SmokePool);var image=frames.gameObject.AddComponent<Image>();
            var animation=frames.gameObject.AddComponent<FrameAnim>();animation.ResizeFrames=false;animation.Begin(image,"bullet_dashe-1_",0,false);frames.sizeDelta=new Vector2(375,80);
            check(image.sprite&&Mathf.Abs(frames.sizeDelta.x-375)<.001f&&animation.Duration>0,
                "helper parity: actual tentacle sequence supports stretched target length rather than a burst at the helper");
            var tentacleFrames=new Sprite[11];for(int i=0;i<tentacleFrames.Length;i++)tentacleFrames[i]=UiAssets.Sprite("bullet_dashe-1_"+i.ToString("00"));
            var tentacle=ReadScript("UI/Prefabs/HelperSkills/dashehandani","UISpriteAnimationSequence");int completed=0;
            animation.Configure(image,tentacleFrames,tentacle.F("frameInterval"),false,tentacle.B("isLooping"));animation.Completed=()=>completed++;
            for(int i=0;i<20;i++)animation.Tick(1f/60f);
            check(!animation.Finished&&completed==0,"helper parity: a 60fps tentacle has not finished after nominal frames-times-interval duration");
            animation.Tick(1f/60f);animation.Tick(1f/60f);animation.Tick(1);
            check(animation.Finished&&completed==1&&image.sprite==tentacleFrames[10],
                "helper parity: tentacle finishes on the real final sequence step and invokes its callback exactly once");

            game.Coins.Add(coin);coinRect.anchoredPosition=((RectTransform)basket.transform).anchoredPosition;
            var basketHits=(HashSet<int>)typeof(SkillShot).GetField("seen",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(basket);
            for(int i=0;i<999;i++)basketHits.Add(i);
            basket.Tick(0);
            check(basket.Finished&&!coin.Busy&&basketHits.Count==999,"helper parity: a basketball ends at 999 prior hits without throwing a 1000th idle coin");

            var football=Shot(game,SkillShot.Kind.Football,0);
            var footballHits=(HashSet<int>)typeof(SkillShot).GetField("seen",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(football);
            game.Coins.Clear();for(int i=0;i<998;i++)footballHits.Add(i);football.Tick(0);
            check(!football.Finished,"helper parity: a football with only 998 prior hits still has its final hit available");
            footballHits.Add(998);coinRect.anchoredPosition=Vector2.zero;game.Coins.Add(coin);football.Tick(0);
            check(football.Finished&&!coin.Busy,"helper parity: football shares the original 999-hit lifetime and never throws a 1000th coin");

            var landedField=typeof(CoinActor).GetField("Landed",BindingFlags.Instance|BindingFlags.NonPublic);
            coin.World=2;HelperSkills.RepeatHaalandThrow(game,coin);
            check(landedField.GetValue(coin)==null,"helper parity: the public Haaland repeat function cannot register a dice-world repeat handler");
            coin.World=1;HelperSkills.RepeatHaalandThrow(game,coin);
            var handler=(Action<CoinActor>)landedField.GetValue(coin);
            var flyingField=typeof(CoinActor).GetField("flying",BindingFlags.Instance|BindingFlags.NonPublic);flyingField.SetValue(coin,true);
            check(handler!=null,"helper parity: Haaland registers one completion handler for a coin-world throw");
            handler(coin);flyingField.SetValue(coin,false);
            check(landedField.GetValue(coin)==null,"helper parity: Haaland removes its completion handler even when the completed coin is already busy");

            coinRect.anchoredPosition=new Vector2(100,0);var tornado=Shot(game,SkillShot.Kind.Tornado,100);tornado.Tick(.1f);
            check(tornado.Finished&&!coin.Busy,"helper parity: a tornado reaching maximum travel ends before polling its final-frame target");

            game.Coins.Clear();
            foreach(float x in new[]{99f,101f,600f,610f}) {
                var target=GameController.NewRect("Density target",game.Field);target.anchoredPosition=new Vector2(x,0);
                var candidate=target.gameObject.AddComponent<CoinActor>();candidate.Game=game;candidate.World=1;candidate.Quality=1;game.Coins.Add(candidate);
            }
            var dense=(List<Vector2>)typeof(HelperSkills).GetMethod("IronTargets",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{game,true});
            check(dense.Count==2&&dense.Contains(new Vector2(99,0))&&dense.Contains(new Vector2(101,0)),
                "helper parity: the 120-cell tie selects the first 99/101 cluster rather than the incorrect 100-cell 600/610 cluster");
        } finally {UnityEngine.Object.DestroyImmediate(host);UnityEngine.Random.state=randomState;}
    }
    static SkillShot Shot(GameController game,SkillShot.Kind kind,float distance) {
        var rect=GameController.NewRect("Test projectile",game.SmokePool);var shot=rect.gameObject.AddComponent<SkillShot>();
        shot.Begin(game,Vector2.right,1000,distance,80,0,7,0,1,1,false,"bullet_jikun",-1,null);shot.SetMode(kind);return shot;
    }
    static Dictionary<string,object> ReadScript(string resource,string script) {
        var data=(Dictionary<string,object>)MiniJson.Parse(Resources.Load<TextAsset>(resource).text);
        foreach(Dictionary<string,object> node in data.Arr("nodes"))foreach(Dictionary<string,object> component in node.Arr("c"))if(component.Str("t")==script)return component;
        throw new InvalidOperationException("Missing original script "+script+" in "+resource);
    }
}
