// Editable SVG compositions retain the exact game screenshots and generated lettering.
const fs=require('fs'),path=require('path');
const sharp=require(require.resolve('sharp',{paths:['C:/Users/aaa/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules']}));
const root=path.resolve(__dirname,'../..'),out=path.join(root,'Marketing/GooglePlay/2026-10-21');
const assets=path.join(out,'Assets'),sources=path.join(out,'SourceScreens'),layouts=path.join(out,'Layouts'),upload=path.join(out,'Upload');
for(const d of [layouts,path.join(upload,'Portrait'),path.join(upload,'Landscape')])fs.mkdirSync(d,{recursive:true});
const capture=JSON.parse(fs.readFileSync(path.join(sources,'capture-result.json'),'utf8'));if(!capture.passed)throw Error(capture.error);
const uri=p=>'data:image/png;base64,'+fs.readFileSync(p).toString('base64');
const img=(p,x,y,w,h,fit='xMidYMid meet')=>`<image x="${x}" y="${y}" width="${w}" height="${h}" preserveAspectRatio="${fit}" href="${uri(p)}"/>`;
const shot=n=>path.join(sources,n),art=n=>path.join(assets,n+'.png');
const native=n=>path.join(root,'Assets/SourceArt/SpriteSheetSources/UI/Sprites',n+'.png');
const crop=(file,x,y,w,h,box)=>`<svg x="${x}" y="${y}" width="${w}" height="${h}" viewBox="${box.join(' ')}" preserveAspectRatio="xMidYMid meet">${img(file,0,0,540,960)}</svg>`;
const defs=`<defs><linearGradient id="gold" x2="0" y2="1"><stop stop-color="#ffe542"/><stop offset="1" stop-color="#ffc91b"/></linearGradient><linearGradient id="water" x2="0" y2="1"><stop stop-color="#70cbe7"/><stop offset="1" stop-color="#7f9ee8"/></linearGradient><pattern id="triangles" width="72" height="64" patternUnits="userSpaceOnUse"><path d="M8 8L48 30L8 52Z" fill="#fff6b0" opacity=".14"/></pattern></defs>`;
const svg=(w,h,title,body)=>`<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}"><title>TycoonRush - ${title}</title>${defs}${body}</svg>`;
function footer(portrait,caption,sub){
  if(portrait)return `<path d="M0 1580L1080 1508V1920H0Z" fill="url(#gold)"/><path d="M0 1580L1080 1508" stroke="#4a2314" stroke-width="22"/><path d="M0 1574L1080 1502" stroke="#fff3c1" stroke-width="11"/><path d="M0 1580L1080 1508V1920H0Z" fill="url(#triangles)"/>${img(art(caption),60,1610,960,160)}<text x="62" y="1842" font-family="Arial,sans-serif" font-size="32" font-weight="700" fill="#653a1d">${sub}</text>${img(art('wordmark'),735,1805,285,86)}`;
  return `<path d="M0 910L1920 840V1080H0Z" fill="url(#gold)"/><path d="M0 910L1920 840" stroke="#4a2314" stroke-width="22"/><path d="M0 904L1920 834" stroke="#fff3c1" stroke-width="11"/><path d="M0 910L1920 840V1080H0Z" fill="url(#triangles)"/>${img(art(caption),65,916,1410,140)}${img(art('wordmark'),1545,970,310,78)}`;
}
const cards=[
 {id:'01-flip-your-fortune',caption:'caption-flip',title:'Flip Your Fortune',source:'01-flip-coins.png',sub:'Tap. Flip. Keep growing.',zoom:[0,230,540,300],background:'#b86079'},
 {id:'02-build-your-team',caption:'caption-team',title:'Build Your Team',source:'02-helper-team.png',sub:'Helpers with unique skills.',zoom:[0,175,540,485],background:'#8977c6'},
 {id:'03-upgrade-your-coins',caption:'caption-upgrade',title:'Upgrade Your Coins',source:'03-coin-upgrades.png',sub:'From bronze to gold.',zoom:[0,520,540,360]},
 {id:'04-explore-two-worlds',caption:'caption-worlds',title:'Explore Two Worlds',source:'04-coin-world.png',sub:'Coins and dice. Two ways to grow.',worlds:true}
];
async function render(doc,name,w,h){
 fs.writeFileSync(path.join(layouts,name+'.svg'),doc);
 const dest=path.join(upload,w===1080?'Portrait':'Landscape',name+'.png');
 await sharp(Buffer.from(doc)).flatten({background:'#6b3c43'}).removeAlpha().png({compressionLevel:9}).toFile(dest);
 return {file:path.relative(root,dest).replaceAll('\\','/'),width:w,height:h,mode:'RGB',layout:path.relative(root,path.join(layouts,name+'.svg')).replaceAll('\\','/')};
}
(async()=>{
 const records=[];
 for(const c of cards){
  const p=shot(c.source);let portrait,landscape;
  if(c.worlds){
   portrait=`<rect width="1080" height="1920" fill="url(#water)"/><ellipse cx="540" cy="720" rx="380" ry="60" fill="#5da9dd" opacity=".35"/><ellipse cx="540" cy="1460" rx="380" ry="60" fill="#5da9dd" opacity=".35"/>${img(native('world_pic_island_01_x2'),110,75,860,690)}${img(native('world_pic_island_02_x2'),110,835,860,690)}`;
   landscape=`<rect width="1920" height="1080" fill="url(#water)"/><ellipse cx="500" cy="750" rx="350" ry="55" fill="#5da9dd" opacity=".35"/><ellipse cx="1440" cy="750" rx="350" ry="55" fill="#5da9dd" opacity=".35"/>${img(native('world_pic_island_01_x2'),100,70,800,700)}${img(native('world_pic_island_02_x2'),1040,70,800,700)}`;
  }else{
   portrait=img(p,0,0,1080,1920);
   landscape=`<rect width="1920" height="1080" fill="${c.background||'#957b69'}"/><rect x="43" y="10" width="520" height="925" rx="18" fill="#4a2314"/>${img(p,52,20,502,893)}${crop(p,630,30,1230,820,c.zoom)}`;
  }
  records.push(await render(svg(1080,1920,c.title,portrait+footer(true,c.caption,c.sub)),c.id+'-1080x1920',1080,1920));
  records.push(await render(svg(1920,1080,c.title,landscape+footer(false,c.caption,c.sub)),c.id+'-1920x1080',1920,1080));
 }
 fs.writeFileSync(path.join(out,'composition-manifest.json'),JSON.stringify({brand:'TycoonRush',reference:'https://play.google.com/store/apps/details?id=com.buffstudio.tower.zombies.defense.idle.game',approach:'One genuine game feature per image. Yellow angled caption strip. Exact captured UI is embedded unchanged in editable SVG layouts; decorative lettering is generated. World images use the original transparent island sprites, matching the two captured island views.',native_world_assets:['world_pic_island_01_x2','world_pic_island_02_x2'],cards,outputs:records},null,2));
 console.log('STORE_GRAPHICS_READY',records.length);
})().catch(e=>{console.error(e);process.exit(1)});
