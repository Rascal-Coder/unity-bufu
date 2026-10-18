"""Build the runtime catalog from the original language table and reviewed corrections.

Run with Python 3 from the repository root. No translation/network service is used.
"""
import json
import pathlib
import re

ROOT = pathlib.Path(__file__).resolve().parent.parent

# Stable keys, complete source messages and their English counterparts.
# The original table's final rows contain shifted translations, so do not import those.
OVERRIDES = r"""
settings.language|语言|Language
settings.title|设置|Settings
settings.music|音乐|Music
settings.sound|音效|Sound
settings.reset|清除存档|Clear Save
settings.reset_confirm|确定要清除存档吗？\n清除后无法恢复|Clear your save?\nThis cannot be undone.
nav.items|物品|Items
nav.helpers|帮手|Helpers
nav.talents|天赋|Talents
nav.shop|商城|Shop
nav.world|世界|Worlds
nav.facility|设施|Facilities
nav.desks|桌面|Tables
nav.money|金钱|Coins
nav.special|特殊|Special
nav.skills|技能|Skills
nav.enhance|强化|Boosts
action.cancel|取消|Cancel
action.confirm|确定|Confirm
action.close|关闭|Close
action.go|前往|Go
action.upgrade|升级|Upgrade
action.summon|召唤|Summon
action.use|使用|Use
action.using|正在使用|In Use
action.reset|重置|Reset
action.rebirth|重生|Rebirth
action.claim|领取奖励|Claim Reward
action.double|双倍获取|Double Reward
action.unlock|解锁|Unlock
action.more|更多|More
action.fold|收起|Less
status.unlocked|已解锁|Unlocked
status.max|已达最大等级|Max Level
status.capped|已达上限|Max Level
status.max_level|最大等级|Max Level
status.soon|敬请期待|Coming Soon
status.unavailable|该功能暂未开放|This feature is not available yet.
status.no_gold|金币不足|Not enough coins.
status.no_diamonds|钻石不足|Not enough diamonds.
status.no_shards|金币或碎片不足|Not enough coins or shards.
status.materials|材料不足|Missing Materials
status.today_limit|今日次数已用完|No attempts left today.
status.purchase_limit|今日购买次数已达上限|Daily purchase limit reached.
status.wheel_progress|转盘进度未满|Fill the spin meter first.
status.claimed|已领取|Claimed
status.claim_early|还没有到领取时间|This reward is not available yet.
status.evolved|进化完成!|Evolution complete!
growth.label|成长等级|Growth Level
growth.unlocked|完成成长等级{0}后解锁|Unlocks after Growth Lv. {0}
growth.goal|恭喜达成<color=#f8642a>{0}</color>小目标！|Goal reached: <color=#f8642a>{0}</color>!
growth.reward_notice|{0}进化！收益 ×10|{0} evolved! Earnings x10
growth.news_headline|<color=#c1695d>震惊！</color>本地知名企业家再度“野蛮生长”！|<color=#c1695d>Breaking!</color> Local tycoon strikes again!
growth.news_body|【头条】夭寿啦！新晋财阀疯狂吞并，小镇格局一夜变天\n独家线报：某神秘玩家名下资产正“无性繁殖”。小镇多年市场格局宣告寿终正寝。|TOWN TIMES: A new tycoon takes over!\nOur sources report that a mysterious investor's fortune keeps multiplying. The town will never be the same.
honor.01|街头穷光蛋|Street Rookie
honor.03|小镇暴发户|Town Tycoon
honor.04|职场老油条|Office Pro
honor.06|百万新贵族|Rich Elite
honor.08|跨国大财团|Global Tycoon
honor.09a|华尔街巨鳄 I|Market Shark I
honor.09b|华尔街巨鳄 II|Market Shark II
honor.09c|华尔街巨鳄 III|Market Shark III
honor.10a|蓝星掌控者 I|World Ruler I
honor.10b|蓝星掌控者 II|World Ruler II
honor.10c|蓝星掌控者 III|World Ruler III
honor.11a|太阳系矿主 I|Solar Baron I
honor.11b|太阳系矿主 II|Solar Baron II
honor.11c|太阳系矿主 III|Solar Baron III
honor.12a|银河大买办 I|Star Magnate I
honor.12b|银河大买办 II|Star Magnate II
honor.12c|银河大买办 III|Star Magnate III
honor.13a|宇宙大财阀 I|Cosmic Mogul I
honor.13b|宇宙大财阀 II|Cosmic Mogul II
honor.13c|宇宙大财阀 III|Cosmic Mogul III
honor.14a|维度收割者 I|Realm Reaper I
honor.14b|维度收割者 II|Realm Reaper II
honor.14c|维度收割者 III|Realm Reaper III
honor.16a|奇点印钞机 I|Apex Minter I
honor.16b|奇点印钞机 II|Apex Minter II
honor.16c|奇点印钞机 III|Apex Minter III
check.title|7日签到|7-Day Check-In
check.free|免费奖励|Free Rewards
check.ad|广告奖励|Ad Rewards
check.day|第{0}天|Day {0}
check.acquire|获取|Get
check.subscribe|订阅|Subscribe
check.subscribe_tip|提前订阅，第三天免费领取强力帮手!|Subscribe early to claim a powerful helper free on Day 3!
check.ad_watch|观看广告|Watch Ad
check.daily_free|每日免费|Daily Free
world.title|选择世界|Choose a World
world.hint|上下滑动，点击岛屿切换世界|Scroll and tap an island to switch worlds.
world.items|道具|Items
world.exclusive|专属帮手|Exclusive Helper
world.more|更多世界|More Worlds
world.more_soon|更多世界敬请期待|More worlds are coming soon!
world.current|当前|Current
world.same|当前已处于该世界|You are already in this world.
world.locked|该世界尚未解锁|This world is still locked.
world.confirm|是否确定切换至{0}？|Switch to {0}?
world.lock_goal|{0}完成<color=#c30505>{1}</color>级后解锁|Unlocks after {0} reaches Lv. <color=#c30505>{1}</color>
world.future|未来城市|Future City
world.dice|骰子世界|Dice World
world.guide_open|骰子世界开放,点击下方按键进入|Dice World is open! Tap below to enter.
world.guide_switch|点击切换至骰子世界|Tap to enter Dice World
helper.unlock_dice|在骰子世界{0}级解锁|Unlocks at Dice World Lv. {0}
helper.unlock_growth|解锁条件\n成长等级达到{0}级|Unlock: Reach Growth Lv. {0}
helper.unlock_level|{0}级解锁|Unlocks at Lv. {0}
helper.level_unlock|LV{0}解锁|Unlocks at Lv. {0}
helper.level_unlock_dot|LV.{0}解锁|Unlocks at Lv. {0}
helper.limit|{0}数量上限+1|{0} capacity +1
helper.income|{0}翻硬币收益：|{0} coin earnings:
helper.range|{0}范围|Range {0}
helper.projectiles|{0}颗|{0}
helper.count|{0}个|{0}
helper.times|{0}倍|{0}x
helper.limit_up|增加上限|Raise Limit
helper.awake|觉醒技能|Awakened Skill
helper.skills|技能收益|Skill Earnings
helper.skill_upgrade|技能升级|Skill Upgrade
helper.base_income|基础金币收益|Base Coin Earnings
helper.name|帮手名字|Helper
helper.name_alt|帮手名称|Helper
helper.assignable|可分配帮手|Available Helpers
helper.coin_skill|帮手翻硬币收益：|Helper coin earnings:
helper.levels|特有技能提升对应等级|Special skill upgrade levels
helper.no_shards|去商城补充一些碎片吧|Get more shards in the shop.
helper.shards|{0}碎片|{0} Shards
helper.love_orbs|发射SSS颗爱心光球，\n随机抛起路径上硬币|Fire SSS heart orbs,\ntossing coins along their path.
helper.love_orbs_inline|发射SSS颗爱心光球，随机抛起路径上硬币|Fire SSS heart orbs, tossing coins along their path.
talent.available|可用天赋|Talent Points
talent.gain|获得天赋点|Get Points
talent.max|天赋已到达最大等级|Talent is at max level.
talent.max_alt|已提升到最大等级|Already at max level.
talent.locked|天赋点系统解锁|Unlock through Talents
talent.rebirth_tip|重生可获取天赋点|Rebirth grants Talent Points.
talent.no_upgrade|金钱未达到升级目标|Upgrade goal not met.
talent.no_reset|当前可回收天赋点为0,无法重置|No spent Talent Points to refund.
talent.no_rebirth|金钱未达到重生目标|Rebirth coin goal not met.
talent.extra|额外+{0}|Extra +{0}
talent.rebirth_max|重生已达上限|Rebirth limit reached
talent.global|全局收益*{0}倍|Global earnings x{0}
talent.global_percent|全局收益*{0}%|Global earnings x{0}%
talent.global_locked|未达到全局收益条件|Global earnings goal not met.
talent.rebirth_desc|重生后，所有金币和等级将清空。<br>但你会获得天赋点和全局收益加成！|Rebirth resets coins and upgrade levels.<br>You gain Talent Points and a global earnings bonus!
talent.rebirth_desc_break|重生后，所有金币和等级将清空。<br>\n但你会获得天赋点和全局收益加成！|Rebirth resets coins and upgrade levels.<br>You gain Talent Points and a global earnings bonus!
talent.next|下个目标：|Next goal:
talent.points_now|当前可获得天赋点：|Talent Points to gain:
talent.regular|普通重生|Standard Rebirth
talent.advanced|进阶重生|Advanced Rebirth
talent.regular_desc|普通重生会清空所有升级项等级|Standard Rebirth resets all upgrade levels.
talent.advanced_desc|进阶重生可保留升级项等级！|Advanced Rebirth keeps upgrade levels!
talent.keep_levels|是否保留等级？|Keep upgrade levels?
talent.reset_confirm|确定要重量天赋树，<br>回收所有已花费的天赋点吗?|Reset this talent tree<br>and refund all spent Talent Points?
talent.reset_confirm_break|确定要重量天赋树，\n<br>回收所有已花费的天赋点吗?|Reset this talent tree<br>and refund all spent Talent Points?
talent.refund|可回收天赋：|Points to refund:
evolve.title|{0}进化|{0} Evolution
evolve.title_alt|青铜硬币进化|Bronze Coin Evolution
evolve.explain|进化后,所有{0}将合成为1个更强的{1},新{2}收益提升{3}倍|Merge all into one stronger {0}.\nEarn {3}x as much.
evolve.preview|{0}收益*<color=#1a9211>{1}</color>倍|{0} earnings x<color=#1a9211>{1}</color>
evolve.old_explain|进化后，所有铜币将合成为1个更强的铜币。|Combine all Bronze Coins into one stronger coin.
evolve.purple|紫币|Purple Coin
evolve.purple_dice|紫骰子|Purple Die
evolve.action|进化|Evolve
item.lock_level|{0}等级达到Lv{1}|{0} reaches Lv. {1}
item.lock_count|需要 {0} 个|Requires {0}
item.unknown|未知条件|Unknown requirement
item.shard_count|{0}个N级碎片|{0} N Shards
item.shard_count_r|{0}个R级碎片|{0} R Shards
item.shard_count_sr|{0}个SR碎片|{0} SR Shards
item.chest_normal|包含：{0}个N级碎片和<color=#e4771e>{1}个R级碎片</color>|Contains: {0} N Shards and <color=#e4771e>{1} R Shards</color>
item.chest_rare|包含：<color=#e4771e>{0}个R级碎片</color>和<color=#9227dd>{1}个SR碎片</color>|Contains: <color=#e4771e>{0} R Shards</color> and <color=#9227dd>{1} SR Shards</color>
item.contents|包含：{0}|Contains: {0}
item.and|{0}和{1}|{0} and {1}
item.generic_count|{0}个{1}|{0} {1}
item.rich_color|<color={0}>{1}</color>|<color={0}>{1}</color>
item.power|{0}*{1}倍|{0} x{1}
item.coin_power|{0}*{1}%|{0} x{1}%
item.lock_gold|获取<color=#e47716>{0}</color>金币后解锁|Unlocks at <color=#e47716>{0}</color> coins
item.talent_boost|天赋系统提升|Boost through Talents
item.helper_upgrade|帮手升级|Helper Upgrade
item.growth_label|解锁条件：主线任务达到{0}级|Unlock: Reach Growth Lv. {0}
item.growth_label_color|<color=#ffeec9>解锁条件</color>\n主线任务达到{0}级|<color=#ffeec9>Unlock</color>\nReach Growth Lv. {0}
item.dice_swipe|滑动可抛起铜骰|Swipe to toss Bronze Dice
item.desk_stack|所有桌面增益效果，可叠加生效|All table bonuses stack.
item.desk_stack_alt|所有桌面增益效果可叠加生效|All table bonuses stack.
item.desk_using|在场景中使用此桌面|Use this table in the scene
shop.go|去商城|Go to Shop
shop.claim|领取|Claim
shop.roll|转动|Spin
shop.sr|随机SR碎片|Random SR Shards
shop.shards|碎片不足|Not Enough Shards
offline.title|离线收益|Offline Earnings
offline.time|{0}小时{1}分|{0}h {1}m
offline.cap|离线时间上限：6小时|Offline limit: 6 hours
offline.earned|你在离线期间获得了|While you were away, you earned:
reward.title|恭喜获得|Rewards
support.money|大富翁|Tycoon
support.income|赚钱大师|Earnings Master
support.speed|鼓舞大师|Cheer Master
support.desc_income|所有帮手获取金钱<color=#e66e17>+{0}%</color>；<br>持续<color=#e66e17>{1}min</color>|All helpers earn <color=#e66e17>+{0}%</color> coins<br>for <color=#e66e17>{1} min</color>.
support.desc_speed|所有帮手速度提高<color=#e66e17>{0}%</color>；<br>持续<color=#e66e17>{1}min</color>|Helper speed <color=#e66e17>+{0}%</color><br>for <color=#e66e17>{1} min</color>.
support.cooldown|过{0}后使用|Available in {0}
support.cheer|加油!加油!|Go! Go!
support.earn|赚钱|Earn!
ad.intro|观看广告获得奖励\n倒计时结束后可关闭广告|Watch to earn a reward.\nClose the ad when the timer ends.
ad.close|关闭广告|Close Ad
ad.complete|广告播放完成！\n点击关闭按钮获取奖励|Ad complete!\nTap Close Ad to claim your reward.
promotion.tip|60秒内显示搜集足够多的金钱!|Collect enough coins within 60 seconds!
promotion.enter|点击进入挑战,获得等级晋升!|Enter the challenge to earn a promotion!
promotion.challenge|挑战|Challenge
promotion.exit|退出挑战|Exit Challenge
promotion.win|挑战成功|Challenge Complete!
promotion.lose|挑战失败|Challenge Failed
promotion.new|新事件|New Event
promotion.time|倒计时|Time Left
dev.refilled|调试资源已补充，签到和进度宝箱已重置|Resources refilled. Check-in rewards and the spin chest have been reset.
dev.title|DEV · 独立调试存档|DEV - Separate Test Save
dev.hint|全部现有内容已解锁\nF2 补资源 · F1 打开/关闭|All available content is unlocked.\nF2: Refill - F1: Open / Close
dev.refill|补充资源 / 重置领取|Refill / Reset Claims
dev.switch|切换硬币 / 骰子世界|Switch Coin / Dice World
dev.reset|重置开发存档（全部解锁）|Reset Test Save (All Unlocked)
guide.fortune.title|开始致富！|Your Fortune\nBegins!
guide.fortune.body|恭喜赚到第一笔钱，还雇到了第一个帮手！\n你的致富之路，从现在开始！|Congrats! You earned your first coins\nand hired your first Helper.\nYour road to riches starts now!
guide.fortune.gift|新手礼|Starter Gift
guide.fortune.start|开始致富|Start Getting Rich
"""


def normalize(value):
    return value.replace('\\n', '\n').replace('\r', '').strip()


def build():
    rows = json.loads((ROOT / 'Assets/Resources/Config/tblanguage.json').read_text(encoding='utf-8'))['rows']
    entries = []
    seen = set()
    for row in rows:
        if row['id'] > 752:
            continue
        source = normalize(row['chinese'])
        if source in seen:
            continue
        seen.add(source)
        entries.append(dict(key=f'original.{row["id"]:03}', zh=source, en=normalize(row['english'])))
    for line in OVERRIDES.strip().splitlines():
        key, source, english = line.split('|', 2)
        entries.append(dict(key=key, zh=normalize(source), en=normalize(english)))
    # Normalize the two original spellings of skill values in the English template.
    for entry in entries:
        entry['en'] = entry['en'].replace('×', 'x')
        if re.search(r'[sS]{3}', entry['zh']):
            spelling = re.search(r'[sS]{3}', entry['zh'])[0]
            entry['en'] = re.sub(r'[sS]{3}', spelling, entry['en'])
    target = ROOT / 'Assets/Resources/Localization/catalog.json'
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(dict(languages=['en'], entries=entries), ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(f'Wrote {len(entries)} entries to {target}')


if __name__ == '__main__':
    build()
