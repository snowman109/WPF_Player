# WPF_Player
利用C#的WPF制作的一款时钟播放器，能够固定在桌面，类似于桌面插件，并且任务栏和alt+tab中不会出现图标，返回桌面的时候也不会最小化，完美地实现了插件该有的特征。

# 使用方法
- 下载项目后，解压wpf_player.7z
- 修改get_list.bat，文件，文件内容为"java -jar NeteaseApi.jar xxxxxxx"，**将xxxxxx换成自己的用户id**。注意，要有java环境。
- 运行wyy.py，命令为"python wyy.py"，**运行之前将wyy.py文件中第167行（倒数第二行）的代码"wyy.login(username='xxxxxxx',password='yyyyyyy')"中xxxxxxx和yyyyyyy分别替换成自己的网易云音乐手机号(一定要是手机号)和密码**.
- 双击打开WPF_Player.exe
- 按照上述步骤运行结束后，会在d盘依次出现：
  - D:\timerconfig\listId.ini  (用于存放当前播放的歌单id)
  - D:\timerconfig\playlist.json  (用于存放用户所有的歌单名称和对应id)
  - D:\timerconfig\songs.json   (用于存放用户所有歌单id和对应歌单的歌曲的名称的id)
  - D:\picture  (用于存放壁纸)

# 注意事项
- ~~上述的歌单json，歌单具体歌曲的json是通过其他工具去获得，传送门：https://github.com/snowman109/NeateaseApi（由于一些问题，不能再用了，新的不再上传）~~
- 由于一些问题，插件只能显示一个歌单，不配置的话默认是喜欢的音乐，如果想听其他歌单可以通过歌单的json去配置歌单id的配置文件。
- 由使用步骤可知，有一个批处理文件和一个python文件，这两个文件分别获取歌单和歌单内的详细歌曲（时间有限，不想整合到一个文件中），只运行python文件即可，在python文件中调用了批处理文件。运行python文件出现各种问题请百度，大多数都是因为缺包，将用到的包列了出来。

``` python
import base64
import codecs
import hashlib
import http.cookiejar
import json
import random
import time
import urllib.request
import subprocess
import requests
from http.cookiejar import LWPCookieJar
from Crypto.Cipher import AES
from bs4 import BeautifulSoup
from fake_useragent import UserAgent
```

# 功能
- 显示时间，日期~~和考研倒计时~~
- ~~自定义显示陌生单词，强化记忆。本着小巧简单的设计原则，所以只能最多支持六个单词。鼠标移动上去会有翻译。~~
- ~~工作提醒功能，这个功能很简单，但是很实用。每小时的50分到下一个小时插件会变成绿色，提醒你休息。之后就是正常颜色，会提醒你工作。~~
- 播放音乐，能够同步到你的网易云音乐歌单，包括自己创建和收藏的。由于本人没学过c#，又着急制作，只能先通过另一个小工具获取到歌单，再进行播放，等有时间再继续制作。
- 更换壁纸，双击插件能够自动更换壁纸。(壁纸要放在D:\picture目录下)
- 快捷键，ctrl+alt+p 暂停和播放；ctrl+alt+→ 下一曲；ctrl+alt+← 上一曲。
- 显示网速，以前一直留着360加速球，只为了看网速情况，但是现在感觉很碍事，干脆自己做一个，就放到了插件中。
- 记忆播放记录，在退出后能保存播放记录，再次启动时会读取记录。
- 新增播放模式，支持顺序播放，随机播放，单曲循环。
- 更改歌单id的时候，无需再重启应用，直接点击设置中的reload按钮即可完成歌单重载。
- 加入日志，可以在软件当前目录的log文件夹下查看日志，日志以日期命名。

# 配置
- ~~单词默认放到了D盘下的words.txt中，若不存在该文件会自己创建~~
- 音乐信息放到了D盘timerconfig文件夹下，里面有4个文件，分别存放了歌单的json，歌单内具体歌曲json，歌单id的配置文件和保存状态的json(只有运行后才会出现，请不要改动！！！)
- 壁纸保存到D盘下的picture文件夹下

# 已知bug
- combobox下拉框要点击几下才会显示出来，并且代码里会抛出异常。
- 换壁纸好像会闪退，没空查原因了，以后再说

# 展示（这是老版本，看个热闹就好了）
![插件展示](https://raw.githubusercontent.com/snowman109/NeateaseApi/master/show/Snipaste_2020-02-23_10-34-08.png)
![歌曲](https://raw.githubusercontent.com/snowman109/NeateaseApi/master/show/Snipaste_2020-02-23_10-34-27.png)
![设置信息](https://raw.githubusercontent.com/snowman109/NeateaseApi/master/show/Snipaste_2020-02-23_10-35-30.png)
