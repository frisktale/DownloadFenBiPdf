# DownloadFenBiPdf
下载粉笔网pdf试卷

## 前言
这只是一个满足自己需要的练手程序，仅能下载事业单位的智能组卷。欢迎参考源码进行二次开发。

## 使用方法
### 1. 添加配置
  在`FenBiCookie`节点下设置cookie。  
  cookie获取方法为：  
  1. 按下 `f12`打开devtool
  1. 切换到“网络”tab页
  1. 进入 [粉笔网](https://www.fenbi.com/page/home) （没登陆的先登录）
  1. 随便找一条网络请求，点开，找到Cookie项，右键，赋值值，粘贴到配置文件中。
  ![](img\cookie.png)  
  
  在 `DownloadCount`节点下设置一共要下载的pdf个数  

  最终配置文件内容如图：  
  ![](img\config.png)
### 2. 运行程序  
即可在程序所在目录下看到下载好的pdf文件
