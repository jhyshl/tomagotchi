# 像素拓麻歌子 Unity 独立版迁移起步包

这不是完整 v0.44.3 的最终移植，而是一个可打开、可继续开发的 Unity 起步工程：

- 已从 `酒馆助手脚本-像素拓麻歌子v0.44.3.json` 提取 6 个内置宠物品种、成长像素帧、颜色变体。
- 已做一个 Unity 运行时原型：领养随机宠物、像素渲染、离线状态衰减、喂食/玩耍/清洁/睡觉、本地 PlayerPrefs 存档。
- 原酒馆脚本完整副本放在 `Assets/StreamingAssets/Original/`，后续继续迁移商城、背包、日程、小游戏、抽奖、排行榜时可以对照。

## 打开方式

1. 安装 Unity Hub 和 Unity 2022.3 LTS 或更高版本。
2. 通过 Unity Hub 选择 `Open project from disk` 打开本目录。
3. 打开后点击菜单：`PixelTamagotchi / 创建启动场景`。
4. 点击 Play 测试。

## 打 Android APK

1. Unity Hub 安装 Android Build Support、SDK/NDK、OpenJDK。
2. Unity 内选择 `File > Build Settings / Build Profiles`，平台切到 Android。
3. Build 输出 APK；要上架 Google Play 则改成 AAB 并配置签名。

## 上传 Git 后能不能自动打包？

可以，但仓库只负责保存源码。要自动产出 APK，需要再配置 Unity Build Automation 或 GitHub Actions + Unity License。建议先手动本地 Build 跑通，再接 CI。

## 后续正式移植建议顺序

1. 数据层：宠物、物品、背包、成就、图鉴。
2. 养成层：日程、课程、工作、随机事件、死亡/复活。
3. UI 层：还原掌机外壳、商城、衣柜、记录、设置。
4. 小游戏层：先迁移 2048/贪吃蛇/扫雷，再迁移宠物奇旅/三维弹球等复杂游戏。
5. 联网层：排行榜、昵称、设备玩家 ID、防刷分签名。
