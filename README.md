# WPF_Player
利用C#的WPF制作的一款时钟播放器，能够固定在桌面，类似于桌面插件，并且任务连和alt+tab中不会出现图标。

# 功能
- 显示时间，日期和考研倒计时
- 自定义显示陌生单词，强化记忆。本着小巧简单的设计原则，所以只能最多支持六个单词。鼠标移动上去会有翻译。
- 工作提醒功能，这个功能很简单，但是很实用。每小时的50分到下一个小时插件会变成绿色，提醒你休息。之后就是正常颜色，会提醒你工作。
- 播放音乐，能够同步到你的网易云音乐歌单，包括自己创建和收藏的。由于本人没学过c#，又着急制作，只能先通过另一个小工具获取到歌单，再进行播放，等有时间再继续制作。
- 更换壁纸，双击插件能够自动更换壁纸。

# 配置
- 单词默认放到了D盘下的words.txt中，若不存在该文件会自己创建
- 音乐信息放到了D盘timerconfig文件夹下，里面有4个文件，分别存放了歌单的json，歌单内具体歌曲json，歌单id的配置文件。
- 壁纸保存到D盘下的picture文件夹下

# 注意事项
- 上述的歌单json，歌单具体歌曲的json是通过其他工具去获得，传送门：https://github.com/snowman109/NeateaseApi
- 由于一些问题，插件只能显示一个歌单，不配置的话默认是喜欢的音乐，如果想听其他歌单可以通过歌单的json去配置歌单id的配置文件。

# 已知bug
- 不能显示缓存状况，即在网不好的情况下可能会干着急
- 某些已经没有版权的歌曲也会显示出来，但是并不会播放，而是一直卡在那里，需要手动切歌

# 展示
![插件展示](https://raw.githubusercontent.com/snowman109/NeateaseApi/master/show/Snipaste_2019-03-18_19-11-10.png)
![歌曲](https://raw.githubusercontent.com/snowman109/NeateaseApi/master/show/Snipaste_2019-03-18_19-11-46.png)
如果展示不出来就自己体验下吧- -