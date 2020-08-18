# WPF_Player
利用C#的WPF制作的一款时钟播放器，能够固定在桌面，类似于桌面插件，并且任务连和alt+tab中不会出现图标。完美地实现了插件该有的特征

# 使用方法
- 下载项目后，解压wpf_player.7z
- 在控制台下运行get_list.bat，注意，要有java环境.可以根据自己的id配置自己的歌单，具体见项目中的doc目录下的详细说明文档
- 双击打开WPF_Player.exe

# 功能
- 显示时间，日期~~和考研倒计时~~
- ~~自定义显示陌生单词，强化记忆。本着小巧简单的设计原则，所以只能最多支持六个单词。鼠标移动上去会有翻译。~~
- ~~工作提醒功能，这个功能很简单，但是很实用。每小时的50分到下一个小时插件会变成绿色，提醒你休息。之后就是正常颜色，会提醒你工作。~~
- 播放音乐，能够同步到你的网易云音乐歌单，包括自己创建和收藏的。由于本人没学过c#，又着急制作，只能先通过另一个小工具获取到歌单，再进行播放，等有时间再继续制作。
- 更换壁纸，双击插件能够自动更换壁纸。
- 快捷键，ctrl+alt+p 暂停和播放；ctrl+alt+→ 下一曲；ctrl+alt+← 上一曲。
- 显示网速，以前一直留着360加速球，只为了看网速情况，但是现在感觉很碍事，干脆自己做一个，就放到了插件中。
- 记忆播放记录，在退出后能保存播放记录，再次启动时会读取记录。
- 新增播放模式，支持顺序播放，随机播放，单曲循环。
- 更改歌单id的时候，无需再重启应用，直接点击设置中的reload按钮即可完成歌单重载。

# 配置
- 单词默认放到了D盘下的words.txt中，若不存在该文件会自己创建
- 音乐信息放到了D盘timerconfig文件夹下，里面有4个文件，分别存放了歌单的json，歌单内具体歌曲json，歌单id的配置文件。
- 壁纸保存到D盘下的picture文件夹下
- 歌曲播放记录保存在D盘timerconfig文件夹下，save.json

# 注意事项
- 上述的歌单json，歌单具体歌曲的json是通过其他工具去获得，传送门：https://github.com/snowman109/NeateaseApi
- 由于一些问题，插件只能显示一个歌单，不配置的话默认是喜欢的音乐，如果想听其他歌单可以通过歌单的json去配置歌单id的配置文件。

# 已知bug
- combobox下拉框要点击几下才会显示出来，并且代码里会抛出异常。
- 换壁纸好像会闪退，没空查原因了，以后再说

# 展示
![插件展示](https://raw.githubusercontent.com/snowman109/NeateaseApi/master/show/Snipaste_2020-02-23_10-34-08.png)
![歌曲](https://raw.githubusercontent.com/snowman109/NeateaseApi/master/show/Snipaste_2020-02-23_10-34-27.png)
![设置信息](https://raw.githubusercontent.com/snowman109/NeateaseApi/master/show/Snipaste_2020-02-23_10-35-30.png)
