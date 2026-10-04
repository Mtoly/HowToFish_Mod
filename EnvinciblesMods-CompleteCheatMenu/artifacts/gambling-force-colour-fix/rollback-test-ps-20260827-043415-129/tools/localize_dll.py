import hashlib, json, re, shutil, struct, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SRC = Path(sys.argv[1]).resolve() if len(sys.argv) > 1 else ROOT / 'artifacts' / 'CompleteCheatMenu.dll'
DST = Path(sys.argv[2]).resolve() if len(sys.argv) > 2 else ROOT / 'CompleteCheatMenu.zh-CN.dll'

# High-signal UI phrases. Unknown technical/member strings are intentionally left intact.
PHRASES = {
 'Complete Cheat Menu  v0.6.1':'完整作弊菜单  v0.6.1','Complete Cheat Menu':'完整作弊菜单',
 'Weapons':'武器','Kill score':'击杀分数','Player':'玩家','Movement':'移动','Boat':'船只','Teleport':'传送','Spawn':'生成','Items':'物品','Skins':'外观','Money':'金钱','World':'世界','Progression':'进度','Entities':'实体','Gambling':'赌博','Fishing':'钓鱼','Visuals':'视觉','Keybinds':'按键','Settings':'设置','Diagnostics':'诊断',
 'God mode':'无敌模式','One-shot kills':'一击必杀','Friendly fire':'友军伤害','Heal':'治疗','Feed':'进食','Full reset':'完全重置','Fly mode':'飞行模式','Free camera':'自由镜头','ESP':'透视','Infinite ammo':'无限弹药','Perfect accuracy':'完美精度','Zero recoil':'无后坐力','Rapid fire':'快速射击','Extra projectiles':'额外弹丸','Bullet speed':'子弹速度','Fire where you\'re looking':'朝准星射击','Aimbot':'自动瞄准','Aim assist':'瞄准辅助',
 'Instant hit':'瞬击','Ballistic prediction':'弹道预测','Tracking chance':'追踪概率','Visible aim assist':'可见拉枪','Aim target mode':'目标类型','Lock radius circle':'锁定范围圆','Gravity compensation':'重力补偿','Recoil compensation':'后坐补偿','Distance drop':'距离下调','Screen lock radius':'屏幕锁定半径','Target diagnostics':'目标诊断','Boxes':'方框','Skeletons':'骨架','Health bars':'血条','Visibility state':'可见状态','Held item info':'手持物信息','Item value info':'物品价值信息','Container info':'容器信息', 'Enable all keybinds':'启用全部快捷键','Master switch':'总开关','Menu key':'菜单键','Open / close menu':'打开/关闭菜单','Hotkeys':'快捷键','Apply':'应用','Reset':'重置','Reset all to defaults':'全部恢复默认','Save':'保存','Load':'加载','Select all':'全选','Clear':'清除','Search':'搜索','Filter':'筛选','Sort':'排序','Status':'状态','Options':'选项','General':'常规','Current values':'当前数值','Defaults are captured the first time each slider is read this session.':'每个滑块首次读取时保存本局初始值。',
 'Balance':'余额','Add money':'增加金钱','Remove':'移除','Set to':'设置为','Quick add':'快速增加','Amount':'数量','Quantity':'数量','Cost':'花费','Value':'数值','Worth':'价值','Weight':'重量','Cookness':'熟度','Multiplier':'倍率','Base worth':'基础价值','Item value multiplier':'物品价值倍率','Apply worth':'应用价值','Apply weight':'应用重量','Apply cookness':'应用熟度','Apply kill score':'应用击杀分数','Apply betting':'应用下注倍率',
 'Unlock everything':'全部解锁','Unlock all':'全部解锁','Unlock boat':'解锁船只','Unlock radar':'解锁雷达','Unlock grill':'解锁烤架','Unlock all islands':'解锁所有岛屿','Unlock all pockets':'解锁所有背包格','Unlock all weapon skins':'解锁所有武器外观','Unlock all characters':'解锁所有角色','Grant every bait':'获得所有鱼饵','All baits granted':'已获得所有鱼饵','Finish game':'完成游戏','Manual save':'手动保存','Save now':'立即保存',
 'Go to boat':'前往船只','Bring boat to me':'召回船只','Go to':'前往','Next island':'下一座岛','Previous island':'上一座岛','Teleport':'传送','Bring':'召回','Waypoint':'航点','Waypoints':'航点','Radius':'半径','Max distance':'最大距离',
 'Boss':'首领','Boss heal':'治疗首领','Heal boss':'治疗首领','Kill boss':'击杀首领','Boss immortal':'首领无敌','All creatures':'所有生物','Creatures cleared':'生物已清除','Clear everything':'清除全部','Clear near me':'清除附近','Despawn':'消失','Spawn':'生成','Spawn one':'生成一个','Spawn catalogue':'生成目录','Filter spawnables…':'筛选可生成物…',
 'Boat flight':'船只飞行','Engine power':'引擎功率','Speed limit':'速度上限','Steering':'转向','Steer smoothing':'转向平滑','Anti-rollover':'防翻滚','Horizontal speed':'水平速度','Vertical speed':'垂直速度','Walk speed':'行走速度','Sprint speed':'冲刺速度','Jump force':'跳跃力度','Extra gravity':'额外重力','Acceleration':'加速度','Deceleration':'减速度','Sink speed':'下沉速度',
 'Force colour':'强制颜色','Win chance':'获胜概率','Bet table':'下注表','Bet color':'下注颜色','Red':'红色','Black':'黑色','Green':'绿色','Start a spin manually':'手动开始旋转','Settle now':'立即结算','Skin machine':'外观机器','Arm':'装填','Equip':'装备','Wear':'穿戴','Rarity':'稀有度',
 'Damage numbers':'伤害数字','Blood':'血液','Decals':'贴花','Tracers':'追踪线','Names':'名称','Distance':'距离','Players':'玩家','Creatures':'生物','Items':'物品','Show my character':'显示我的角色','Field of view':'视野','Sensitivity':'灵敏度',
 'Host / solo':'房主/单人','Host or solo only — the server owns this.':'仅限房主或单人，服务器负责此操作。','disabled':'已禁用','enabled':'已启用','on':'开','off':'关','Unavailable':'不可用','None':'无','OK':'确定','Cancel':'取消','Back':'返回','Next ▶':'下一步 ▶','◀ Previous':'◀ 上一步',
 'Save current settings':'保存当前设置','Save here':'保存到此处','Presets':'预设','No presets yet. Presets store movement, ESP, camera and item values.':'暂无预设。预设保存移动、透视、镜头和物品数值。','Re-run checks':'重新检查','Bindings':'绑定','Binding failed: ':'绑定失败：','Diagnostics: {0}/{1} bindings failed.':'诊断：{0}/{1} 个绑定失败。',
 'Ammo refilled':'弹药已补满','God mode on':'无敌模式已开启','God mode off':'无敌模式已关闭','One-shot on':'一击必杀已开启','One-shot off':'一击必杀已关闭','Free camera on':'自由镜头已开启','Free camera off':'自由镜头已关闭','Cheat hotkeys enabled':'作弊快捷键已启用','Cheat hotkeys disabled':'作弊快捷键已禁用','Boat unlocked':'船只已解锁','Radar unlocked':'雷达已解锁','All islands unlocked':'所有岛屿已解锁','All skins locked':'所有外观已锁定','All weapon skins unlocked':'所有武器外观已解锁',
 'A backup copy of your save folder is taken automatically before the first permanent change each session.':'每局首次永久修改前会自动备份存档文件夹。','Click a key to rebind it, then press the new key. Escape cancels.':'点击按键进行重绑，然后按下新按键。按 Esc 取消。',
 'Needs line of sight':'需要视线','Accepts shorthand: 50k, 2.5m, 1b.':'支持简写：50k、2.5m、1b。','Achievements are written to Steam. Locking clears the local flags, but anything already pushed to your Steam profile stays there.':'成就会写入 Steam。锁定只清除本地标记，已同步到 Steam 的记录不会改变。','Aim height lifts the point of aim off the target\'s origin — raise it for head shots on tall creatures.':'瞄准高度会抬高瞄准点，调高可瞄准高大生物的头部。','Aimbot is off.':'自动瞄准已关闭。','Air drag':'空气阻力','Air spin drag':'空气旋转阻力','Aligns the gun\'s fire point with the camera, removing the barrel-to-eye offset that makes hipfire drift off the crosshair. Pair with perfect accuracy for pinpoint hipfire.':'将枪口对齐镜头，消除腰射偏移；配合完美精度实现精准射击。','All baits ({0})':'全部鱼饵（{0}）','Also cancels queued reloads, so the gun never stops to reload.':'同时取消排队换弹，枪械不会停火换弹。','Anything a player is holding or has stored is excluded and never removed.':'玩家手持或存放的物品不会被处理。','Applies base worth 100,000 and weight 10 to whichever target is selected above.':'将选中目标的基础价值设为 100000、重量设为 10。','Arm a skin, then roll — the reel is built around your pick, so the machine visibly stops on the skin it hands you.':'装填外观后旋转，机器会停在你选择的外观上。','Auto-unlock dev cheats':'自动解锁开发者作弊','Back up saves before permanent changes':'永久修改前备份存档','BetColor enum did not resolve. See Diagnostics.':'下注颜色枚举解析失败，请查看诊断。','Bonuses multiply together — the game computes the payout as the product of every row, which is why picking several stacks up fast.':'奖励倍率相乘，选择多个奖励会快速叠加。','Categories come from the components each prefab carries, not from its name.':'类别根据预制体组件判定，而不是名称。','Clears screen kick, weapon kick and the knockback the shot applies to you.':'清除屏幕震动、武器后坐和射击自伤击退。','Cone (degrees)':'锥角（度）','Copy the save folder once per session before the first destructive action. Silent, no prompt.':'每局首次破坏性操作前静默备份存档。','Creatures are found by scanning the scene twice a second — the game keeps no live creature list. Raising the distance costs nothing extra.':'每半秒扫描场景寻找生物，扩大距离不会增加开销。','Engine power is the thrust the motor applies and is the real speed control. The limit is the rigidbody\'s velocity cap — the game only reads its own value at startup, so this writes the cap directly.':'引擎功率决定推力，速度上限直接写入刚体。','Every spin lands on this colour regardless of what you bet. Green pays 35x.':'每次旋转都落在此颜色，绿色赔率为 35 倍。','Free camera needs a live player camera. Load into a save.':'自由镜头需要玩家镜头，请先加载存档。','Full reset also clears poison and fire.':'完全重置还会清除中毒和着火。','Grants ownership without charging — the same call the game makes once a purchase is approved.':'直接授予所有权，不收取费用。','Hold a gun. Settings below are applied to whatever you\'re carrying and re-applied when you switch weapons.':'手持枪械后，以下设置会应用到当前武器并在切换时重新应用。','Hold an item to see and apply its skins.':'手持物品即可查看并应用外观。','Island travel moves everyone on the server, same as the game\'s own command.':'岛屿旅行会移动服务器上的所有玩家。','Master switch for every cheat hotkey. The menu key is unaffected and always works.':'所有作弊快捷键的总开关，菜单键不受影响。','Mouse looks, WASD moves, Space and Ctrl for height, Shift to boost. Your body is now pinned in place while the camera is detached.':'鼠标观察，WASD 移动，空格和 Ctrl 控制高度，Shift 加速；自由镜头期间身体会固定。','No boss active. Spawn one from the Spawn tab.':'当前没有首领，请在生成页生成。','No boat in the scene yet. Load into a save with a boat spawned.':'场景中还没有船，请加载已有船的存档。','No player inventory yet. Load into a save first.':'尚未找到玩家背包，请先加载存档。','Points the gun\'s fire point at the best target while your view stays under your control — your shots go where the aimbot looks, your camera doesn\'t get yanked around.':'枪口会指向最佳目标，但镜头仍由你控制。','Rarity also decides where on the reel it lands — the game seats commons, rares and legendaries in different positions.':'稀有度决定转盘落点，普通、稀有和传奇位于不同位置。','Rolled once per spin. On a win the ball is steered to the colour you bet; on a loss it goes to the other of red or black, never green, so a loss can\'t accidentally pay 35x.':'每次旋转判定一次，赢时落在下注颜色，输时落在另一种红黑色，不会落到绿色。','Skips the drop that normally scatters your items when you die, on both the inventory and the server side so nothing desyncs.':'跳过死亡掉落，客户端与服务器保持同步。','Spawns 2m in front of the camera. Dead and Drip only apply to creatures.':'在镜头前方 2 米生成；死亡和滴水变体仅适用于生物。','The ball is a real physics object and the game reads its actual angle against the wheel to decide the result.':'球体使用真实物理，游戏根据它相对转盘的角度判定结果。','The game has no stack count — every slot holds one real item with its own worth and skin — so this spawns that many genuine copies at your feet instead of faking a stack number.':'游戏没有堆叠数量，每个栏位都是独立物品；此操作会生成对应数量的真实副本。','Turns any gun into a shotgun. Combine with perfect accuracy to stack every pellet on one point.':'将枪械变为霰弹枪，配合完美精度可让所有弹丸命中一点。','WASD moves, Space up, Left Ctrl down. Releasing both hovers instead of falling. Gravity is restored when you switch fly off.':'WASD 移动，空格上升，左 Ctrl 下降；松开后悬停，关闭飞行时恢复重力。','Uses the game\'s own Item.DestroyItem, so objects despawn across the network properly rather than vanishing only on your screen. This will also remove loot and quest items lying on the ground.':'使用游戏自身的销毁接口，物体会在网络中正确消失，也会移除地面战利品和任务物品。',
 'Climb speed':'攀爬速度','Engine power':'引擎功率','Speed limit':'速度上限','Water drag':'水面阻力','Water spin drag':'水面旋转阻力','Air drag':'空中阻力','Air spin drag':'空中旋转阻力','Anti-rollover':'防翻滚','Steer smoothing':'转向平滑','Boat handling reset':'船只操控已重置','Reset handling':'重置操控','Flight':'飞行','Boat flight':'船只飞行','Handling':'操控','Current tier: {0}':'当前等级：{0}','Motor set to {0}':'引擎等级已设为 {0}','Speed':'速度',
 'Creatures':'生物','Dead':'死亡','Distance':'距离','Drip':'滴水','Items':'物品','Names':'名称','Players':'玩家','Tracers':'追踪线','What are you winning a skin for':'你要为哪种对象赢取外观','Pick a rarity, or take any of it':'选择稀有度，或选择任意','Pick a skin':'选择外观','Or choose the exact skin':'或选择指定外观','A–Z':'按字母排序','Filter characters…':'筛选角色…','Filter skins…':'筛选外观…','Find a weapon…':'查找武器…','All achievements unlocked':'全部成就已解锁','All bonuses selected':'已选择全部奖励','All characters unlocked':'全部角色已解锁','All pockets unlocked':'全部背包格已解锁','Boat locked':'船只未解锁','Boat moved':'船只已移动','Boat present':'船只已就位','Character hidden':'角色已隐藏','Character visible':'角色已显示','Cheat hotkeys':'作弊快捷键','Complete drip':'完成滴水日志','Complete normal':'完成普通日志','Could not switch the model':'切换模型失败','Current island {0}':'当前岛屿：{0}','Currently equipped: bait {0}':'当前装备鱼饵：{0}','Currently inactive — you can still rebind them.':'当前未启用，仍可重新绑定。','Dev cheats disabled':'开发者作弊已禁用','Dev cheats enabled':'开发者作弊已启用','Dev cheats unlocked':'开发者作弊已解锁','Developer cheats':'开发者作弊','Disable dev cheats':'禁用开发者作弊','Drip journal completed':'滴水日志已完成','Drip journal reset':'滴水日志已重置','Drop everything':'丢弃全部','Enable ESP':'启用透视','Enable dev cheats':'启用开发者作弊','Every item in world':'世界中的每件物品','Everything loose':'全部散落物','Extra multiplier':'额外倍率','Extra pockets unlocked: {0}':'额外背包格已解锁：{0}','FOV control':'视野控制','Free bullet upgrade':'免费子弹升级','Friendly fire off':'友军伤害已关闭','Friendly fire on':'友军伤害已开启','Fullness +{0}':'饱食度 +{0}','Game completion':'游戏完成度','Game marked finished':'游戏已标记为完成','Ground movement':'地面移动','Heal amount':'治疗量','Health {0}':'生命值 {0}','Held item':'手持物品','Held item skin':'手持物品外观','Held weapon':'手持武器','Holding a weapon':'正在持有武器','How this works':'工作原理','In game':'游戏中','Inventory API':'背包接口','Inventory saved':'背包已保存','Keep inventory on death':'死亡保留背包','Keep inventory: items retained':'背包保留：物品已保留','Keep inventory: skipped the death drop.':'背包保留：已跳过死亡掉落。','Kill score catalogue: {0} bonuses.':'击杀分数目录：{0} 个奖励。','Kill score control':'击杀分数控制','Kill score multi':'击杀分数倍率','Kill scores shown':'击杀分数已显示','Last result':'上次结果','Load into a save first.':'请先加载存档。','Lock all':'全部锁定','Make everything valuable':'让全部物品更有价值','Movement reset':'移动设置已重置','No casino in this scene. Travel to the island with the casino.':'当前场景没有赌场，请前往赌场所在岛屿。','No other players found.':'未找到其他玩家。','No target in the cone.':'锥角内没有目标。','No waypoints yet. They last until you quit the game.':'暂无航点，航点会保留到退出游戏。','Normal journal completed':'普通日志已完成','Normal journal reset':'普通日志已重置','Nothing armed':'未装填任何内容','Nothing has been changed on this weapon yet.':'这把武器尚未修改。','Nothing in hand.':'手中没有物品。','Nothing loose right now.':'当前没有散落物。','One-shot toggle failed':'一击必杀切换失败','Original values restored':'已恢复原始数值','Player brought':'玩家已召回','Pocket {0} unlocked':'背包格 {0} 已解锁','Preset load failed: ':'预设加载失败：','Preset save failed: ':'预设保存失败：','Radar locked':'雷达未解锁','Refill now':'立即补充','Restore this weapon\'s originals':'恢复此武器原始值','Rig cleared':'装填已清除','Roll the machine':'旋转机器','Roulette odds':'轮盘赔率','Save inventory':'保存背包','Selection cleared':'选择已清除','Show kill scores':'显示击杀分数','Spin in progress':'旋转进行中','Spinning on ':'当前落点：','Stacking and inventory':'堆叠与背包','Stats cleared':'统计已清除','Steam achievements':'Steam 成就','Target player has no usable position.':'目标玩家没有可用位置。','Teleported to boat':'已传送到船只','This item has no skin preset.':'此物品没有外观预设。','This tab hit an error.':'此页面发生错误。','Total worth':'总价值','Total worth is derived from base worth, weight and cookness.':'总价值由基础价值、重量和熟度计算。','Vitals reset':'生命状态已重置','Weight set to {0:0.##}':'重量已设为 {0:0.##}','Wheel settled':'转盘已停稳','World cleared':'世界已清理','World unlocks':'世界解锁','You bet':'你的下注','· = needs host':'· = 需要房主','and always works.':'并且始终有效。','disabled — see Keybinds tab':'已禁用，请查看按键页面','the roll is random':'本次结果为随机','press a key…':'请按下按键…','unlocked up to {0}':'已解锁至 {0}','{0} bonuses':'{0} 个奖励','{0} entries':'{0} 个条目','{0} items in the world.':'世界中有 {0} 件物品。','{0} loose object(s) in the world':'世界中有 {0} 个散落物','{0} of {1} skins':'{0}/{1} 个外观','{0} of {1} spawnables':'{0}/{1} 个可生成物','{0} tracked':'已追踪 {0} 个','{0} {1} loaded. Press {2} in game.':'已加载 {0} {1}。进入游戏后按 {2}。','{0} — {1} skins':'{0} — {1} 个外观',
}
WORD = {'Current':'当前','Target':'目标','Selected':'已选','Live':'实时','Total':'总计','Count':'数量','Category':'类别','Categories':'类别','Name':'名称','Names':'名称','ID':'编号','Range':'范围','Distance':'距离','Speed':'速度','Power':'功率','Flight':'飞行','Camera':'镜头','Weapon':'武器','Weapons':'武器','Player':'玩家','Players':'玩家','Boat':'船只','Island':'岛屿','Islands':'岛屿','Skin':'外观','Skins':'外观','Bait':'鱼饵','Baits':'鱼饵','Bonus':'奖励','Bonuses':'奖励','Journal':'日志','Normal':'普通','Drip':'滴水','Complete':'完成','Reset':'重置','Unlock':'解锁','Unlocked':'已解锁','Lock':'锁定','Locked':'已锁定','Show':'显示','Hide':'隐藏','Enable':'启用','Disable':'禁用','Apply':'应用','Restore':'恢复','Original':'原始','Values':'数值','Value':'数值','Worth':'价值','Weight':'重量','Health':'生命','Damage':'伤害','Amount':'数量','Quantity':'数量','Gravity':'重力','Horizontal':'水平','Vertical':'垂直','Fast':'快速','Perfect':'完美','Infinite':'无限','Extra':'额外','Force':'强制','Random':'随机','All':'全部','Every':'每个','World':'世界','Entities':'实体','Items':'物品','Creatures':'生物','Settings':'设置','Options':'选项','Status':'状态','Active':'激活','Inactive':'未激活','Available':'可用','Unavailable':'不可用','Failed':'失败','Error':'错误','Success':'成功','Cleared':'已清除','Cleared':'已清除','Loaded':'已加载','Saved':'已保存','Granted':'已获得','Equipped':'已装备','Holding':'持有','Held':'手持','Selected':'已选择','Nothing':'无','None':'无','All':'全部','Every':'每个','Normal':'普通','Drip':'滴水','Boss':'首领','Creature':'生物','Bird':'鸟类','Fish':'鱼类','Explosive':'爆炸物','Melee':'近战','Tool':'工具','Other':'其他','Hotkeys':'快捷键','Cheat':'作弊','Developer':'开发者','Dev':'开发者','Money':'金钱','Inventory':'背包','Pockets':'背包格','Achievements':'成就','Game':'游戏','World':'世界','Score':'分数','Kill':'击杀','Kills':'击杀','Damage':'伤害','Blood':'血液','Decals':'贴花','Tracers':'追踪线','Names':'名称','Distance':'距离','Current':'当前','Previous':'上一','Next':'下一','Move':'移动','Movement':'移动','Ground':'地面','Water':'水面','Air':'空中','Spin':'旋转','Wheel':'转盘','Ball':'球','Colour':'颜色','Color':'颜色','Red':'红色','Black':'黑色','Green':'绿色','Chance':'概率','Win':'获胜','Loss':'失败','Lost':'失败','Won':'获胜','Result':'结果','Odds':'赔率','Table':'桌面','Item':'物品','Items':'物品','World':'世界','Everything':'全部','Nothing':'无','Clear':'清除','Despawn':'消失','Spawn':'生成','Catalogue':'目录','Filter':'筛选','Sort':'排序','Search':'搜索','Weapon':'武器','Fire':'射击','Bullet':'子弹','Ammo':'弹药','Recoil':'后坐力','Accuracy':'精度','Rapid':'快速','Projectiles':'弹丸','Shot':'射击','Shots':'射击次数','Aim':'瞄准','Aimbot':'自动瞄准','Height':'高度','Cone':'锥角','Line':'线','Sight':'视线','Boat':'船只','Motor':'引擎','Engine':'引擎','Steering':'转向','Throttle':'油门','Handling':'操控','Flight':'飞行','Radar':'雷达','Teleport':'传送','Travel':'旅行','Go':'前往','Bring':'召回','Waypoints':'航点','Waypoint':'航点','Character':'角色','Characters':'角色','Show':'显示','Hidden':'隐藏','Visible':'可见','Hide':'隐藏','Body':'身体','Camera':'镜头','Field':'视野','FOV':'视野','Sensitivity':'灵敏度','Preset':'预设','Presets':'预设','Save':'保存','Load':'加载','Config':'配置','Backup':'备份','Permanent':'永久','Change':'修改','Session':'本局','Automatically':'自动','Before':'之前','First':'首次','Every':'每个','Second':'秒','Seconds':'秒','Once':'一次','Now':'现在','Current':'当前','Originals':'原始值','Defaults':'默认值','Reset':'重置','Restore':'恢复','Use':'使用','Own':'自己的','My':'我的','Your':'你的','The':'该','This':'此','That':'该','And':'和','Or':'或','With':'与','Without':'不含','From':'从','To':'到','For':'用于','On':'开启','Off':'关闭','In':'在','Out':'外','Up':'上','Down':'下','At':'位于','By':'按','Of':'的','Is':'是','Are':'是','Was':'曾为','Has':'有','Have':'有','No':'无','Not':'不','Only':'仅','Also':'还','Always':'始终','Never':'从不','Press':'按下','Click':'点击','Pick':'选择','Choose':'选择','Select':'选择','Hold':'按住','Release':'释放','Enter':'输入','Leave':'离开','When':'当','While':'当','If':'如果','Then':'然后','Each':'每个','Any':'任意','All':'全部','Every':'每个','Real':'真实','Really':'确实','Live':'实时','Local':'本地','Server':'服务器','Client':'客户端','Host':'房主','Solo':'单人','Needs':'需要','Nothing':'无','Could':'可以','Cannot':'无法','Unable':'不可用','Failed':'失败','Resolved':'已解析','Resolve':'解析','See':'查看','Diagnostics':'诊断','Log':'日志'}
NO_TRANSLATE = {'General','Keybinds','Settings','ToggleKey','EnableAllKeybinds','AutoEnableCheats','BackupSaves',
                'Player','Weapons','Movement','Boat','Teleport','Spawn','Entities','Items','Skins','Money','World',
                'Kill score','Progression','Gambling','Fishing','Visuals','Diagnostics'}

def protected_runtime_strings():
    """Protect member/type identifiers that are also ordinary UI words.

    #US strings are shared by value, so translating a reflection identifier such
    as Heal, Value, or Boss also changes GameBinder lookups at runtime.  Derive
    the protected set from source calls instead of maintaining a fragile list.
    """
    protected = set(NO_TRANSLATE)
    source_root = ROOT / 'decompiled-src' / 'CompleteCheatMenu'
    call = re.compile(
        r'(?:GameBinder\.(?:Type|Field|Property|Method)|ReadMember|GetProperty|GetField|GetMethod|GetNestedType|HarmonyPatch)\s*\([^;\n]*'
    )
    quoted = re.compile(r'"([A-Za-z_][A-Za-z0-9_+.`]*)"')
    for path in source_root.rglob('*.cs'):
        text = path.read_text(encoding='utf-8', errors='ignore')
        for match in call.finditer(text):
            protected.update(quoted.findall(match.group(0)))
    return protected

PROTECTED_RUNTIME = protected_runtime_strings()

def translate(s):
    if s in PROTECTED_RUNTIME:
        return s
    t = PHRASES.get(s, s)
    if False and t == s and any(c.isalpha() and c.isascii() for c in s) and ' ' in s:
        for a,b in WORD.items(): t = re.sub(rf'\b{re.escape(a)}\b', b, t)
    return t

def us_entries(data):
    # Locate CLI metadata root and #US stream.
    md_off = data.find(b'BSJB')
    if md_off < 0: raise RuntimeError('metadata root not found')
    b = data[md_off:]
    ver_len = struct.unpack_from('<I', b, 12)[0]
    p = 16 + ver_len
    while p % 4: p += 1
    n = struct.unpack_from('<H', b, p+2)[0]; p += 4; us = None
    for _ in range(n):
        off,size = struct.unpack_from('<II', b, p); q=p+8; end=b.find(b'\0',q); name=b[q:end].decode('ascii','replace')
        if name == '#US': us=(md_off+off, size)
        p += 8 + ((end-q+1+3)//4)*4
    if not us: raise RuntimeError('#US stream not found')
    start,size=us; end=start+size; p=start+1
    while p<end:
        first=data[p]
        if first==0: p+=1; continue
        if first & 0x80 == 0: ln,hdr=first,1
        elif first & 0xC0 == 0x80: ln=((first&0x3f)<<8)|data[p+1]; hdr=2
        else: ln=((first&0x1f)<<24)|(data[p+1]<<16)|(data[p+2]<<8)|data[p+3]; hdr=4
        if ln>1 and p+hdr+ln<=end:
            raw=data[p+hdr:p+hdr+ln]
            try: s=raw[:-1].decode('utf-16le') if len(raw)%2 else raw.decode('utf-16le').rstrip('\0')
            except UnicodeDecodeError: s=''
            yield p+hdr, raw, s
        p += hdr+ln

if not SRC.exists():
    raise FileNotFoundError(f'input assembly not found: {SRC}')
data=bytearray(SRC.read_bytes()); original=bytes(data); changes=[]
for marker in (b'EspRendererV2', b'WeaponStateStore', b'VisibleAimController'):
    if marker not in data:
        raise RuntimeError(f'input assembly is not the current cnKX build; missing type marker: {marker.decode()}')
for pos,raw,s in us_entries(data):
    t=translate(s)
    if t==s: continue
    # Keep the exact UTF-16 code-unit slot so metadata offsets remain valid.
    terminal = raw[-1:] if len(raw) % 2 else b''
    payload_len = len(raw) - len(terminal)
    units=payload_len//2
    t=''.join(ch for ch in t if ord(ch)<=0xffff)
    # Metadata user-string slots are fixed-width. Use a non-whitespace invisible
    # pad so BepInEx ConfigDefinition accepts translated key names.
    t=t[:units].ljust(units,'\u200b')
    enc=t.encode('utf-16le') + terminal
    if len(enc)!=len(raw): continue
    data[pos:pos+len(raw)]=enc
    changes.append({'offset':pos,'original':s,'translated':t.rstrip(),'slot_units':units})
DST.write_bytes(data)
DST.with_suffix('.localization.diff.json').write_text(json.dumps({'source_sha256':hashlib.sha256(original).hexdigest(),'modified_sha256':hashlib.sha256(data).hexdigest(),'changed':changes},ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'changed':len(changes),'source_sha256':hashlib.sha256(original).hexdigest(),'modified_sha256':hashlib.sha256(data).hexdigest()},ensure_ascii=False))


