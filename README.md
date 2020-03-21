# TinaX Framework - Lua.

<a href="https://tinax.corala.space"><img src="https://github.com/yomunsam/TinaX.Core/raw/master/readme_res/logo.png" width = "420" height = "187" alt="logo" align=center /></a>


[![LICENSE](https://img.shields.io/badge/license-NPL%20(The%20996%20Prohibited%20License)-blue.svg)](https://github.com/996icu/996.ICU/blob/master/LICENSE)
<a href="https://996.icu"><img src="https://img.shields.io/badge/link-996.icu-red.svg" alt="996.icu"></a>
[![LICENSE](https://camo.githubusercontent.com/3867ce531c10be1c59fae9642d8feca417d39b58/68747470733a2f2f696d672e736869656c64732e696f2f6769746875622f6c6963656e73652f636f6f6b6965592f596561726e696e672e737667)](https://github.com/yomunsam/TinaX/blob/master/LICENSE)

基于`xLua`的Lua运行环境.

Lua runtime based on `xLua`.

<br>

由于所使用的第三方库`Tencent/xLua`的限制，`TinaX.Lua`只能放在`Assembly-CSharp`中（即直接放在项目Assets目录下的某个地方）。

Due to the limitations of the third-party library `Tencent/xLua` used,` TinaX.Lua` can only be placed in `Assembly-CSharp` (that is, directly placed somewhere in the project's Assets directory).  

<!-- ```
git://github.com/yomunsam/TinaX.Lua.git
```

package name: `io.nekonya.tinax.lua` -->

<br>
------

## Dependencies

This package does not depend on other packages.

在安装之前，请先确保已安装如下依赖：

Before setup `TinaX.Lua`, please ensure the following dependencies are installed by `Unity Package Manager`:

- [io.nekonya.tinax.core](https://github.com/yomunsam/tinax.core) :`git://github.com/yomunsam/TinaX.Core.git`
- [io.nekonya.tinax.xcomponent](https://github.com/yomunsam/TinaX.XComponent) :`git://github.com/yomunsam/TinaX.XComponent.git`

------

## Third-Party

本项目中使用了以下优秀的第三方库：

The following excellent third-party libraries are used in this project:

- **[xLua](https://github.com/Tencent/xLua)** : xLua is a lua programming solution for C# ( Unity, .Net, Mono) , it supports android, ios, windows, linux, osx, etc.
- **[middleclass](https://github.com/kikito/middleclass)** : Object-orientation for Lua.

-------

