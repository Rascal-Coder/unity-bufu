# Compares the current loading flow with the original MainEntry / PreloadManager gates.
import pathlib
import sys

root = pathlib.Path(__file__).resolve().parents[1]
launch = (root / "Assets/Scripts/LaunchScreen.cs").read_text(encoding="utf-8")
boot = (root / "Assets/Scripts/GameController.Boot.cs").read_text(encoding="utf-8")
intro = (root / "Assets/Scripts/IntroScene.cs").read_text(encoding="utf-8")
scenes = sorted(p.name for p in (root / "Assets/Scenes").glob("*.unity"))

failures = []

def check(name, ok, detail=""):
    print(("PASS " if ok else "FAIL ") + name + ((" — " + detail) if detail else ""))
    if not ok:
        failures.append(name)

check("build scenes are Intro then Main", scenes == ["Intro.unity", "Main.unity"], str(scenes))
check("progress opens at 20%", "BaseProgress=.2f" in launch and "screen.Apply(BaseProgress)" in launch)
check("each finished prefab adds 10%", "preloadCount*.1f+BaseProgress" in launch)
check("cdn film address", "https://bsls-cdn.jvplay.cn/m1b/mv.mp4" in intro)
check("film is parented on the loading page", "LaunchScreen.Current.PageRoot" in intro and "IntroVideoContainer" in intro)
check("enter waits for the film and the preload", intro.find("while(!videoDone)") < intro.find("while((GameController.Active==null||!GameController.Active.BootFinished)") < intro.find("PresentAfterIntro()"))
check("fade is the original 1.5s", "FadeAndHide(1.5f)" in (root / "Assets/Scripts/GameController.cs").read_text(encoding="utf-8"))
check("upgrade page is preloaded before later pages", boot.find("LaunchScreen.PreloadFinished()") < boot.find("if(Model.MainLevel>3)"))
check("later pages are facility, shop, talent", "EnsureFacility();LaunchScreen.PreloadFinished()" in boot and "EnsureShop();LaunchScreen.PreloadFinished()" in boot and "EnsureTalent();LaunchScreen.PreloadFinished()" in boot)

BASE = 0.2

def original_progress(level):
    pages = ["upgrade"] if level <= 3 else ["upgrade", "facility", "shop", "talent"]
    values = [BASE]
    for i in range(1, len(pages) + 1):
        values.append(round(BASE + i * 0.1, 1))
    return pages, values

def our_progress(level):
    # Mirrors GameController.Boot: one PreloadFinished after the upgrade page,
    # then three more only when MainLevel > 3.
    pages = ["upgrade"] if level <= 3 else ["upgrade", "facility", "shop", "talent"]
    values = [BASE]
    for i in range(1, len(pages) + 1):
        values.append(round(BASE + i * 0.1, 1))
    return pages, values

def original_enter(video_already, video_done, preload_done):
    video_gate = True if video_already else video_done
    return video_gate and preload_done

def our_enter(video_already, video_done, preload_done):
    # Intro skips the film when the flag is set, then Main boots and presents.
    # Otherwise PresentAfterIntro runs only after videoDone and BootFinished.
    if video_already:
        return preload_done
    return video_done and preload_done

print("\nscenarios")
for level, played in ((1, False), (88, False), (88, True), (2, True)):
    op, ov = original_progress(level)
    mp, mv = our_progress(level)
    same_pages = op == mp and ov == mv
    gates = []
    for video_done, preload_done in ((False, False), (True, False), (False, True), (True, True)):
        gates.append(original_enter(played, video_done, preload_done) == our_enter(played, video_done, preload_done))
    ok = same_pages and all(gates)
    detail = "level %s played %s progress %s pages %s" % (level, played, ov, op)
    check("scenario " + detail, ok, "" if ok else "pages or gates diverged")
    if not played:
        check("main stays hidden until both gates, " + detail, our_enter(False, False, True) is False and our_enter(False, True, True) is True)

print("\n" + ("MATCH" if not failures else "MISMATCH %s" % ", ".join(failures)))
sys.exit(1 if failures else 0)
