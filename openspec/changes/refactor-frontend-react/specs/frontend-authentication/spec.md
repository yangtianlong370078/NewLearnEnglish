## 新增需求

### 需求：登录页面
系统**必须**提供现代化登录页面，包含品牌渐变背景、表单输入（账号/密码）、登录按钮，调用 `/login/loginresult` 接口。

#### 场景：账号密码登录
- **当** 用户输入账号密码并点击登录
- **那么** POST 请求 `/login/loginresult?loginID=xxx&password=xxx&sb=0`
- **当** 返回 `success: true`
- **那么** 存储 token 和用户信息，跳转到首页 `/`
- **当** 返回 `success: false`
- **那么** 显示错误 Toast 提示

#### 场景：未登录访问保护页面
- **当** 用户未登录直接访问 `/words` 等需要认证的页面
- **那么** 自动重定向到 `/login`

### 需求：注册页面
系统**必须**提供注册页面，包含登录ID、密码、手机号、姓名输入，调用 `/login/setregister` 接口。

#### 场景：用户注册
- **当** 用户填写完整信息并提交
- **那么** POST 请求 `/login/setregister?loginID=xxx&password=xxx&phone=xxx&name=xxx`
- **当** 返回成功
- **那么** 显示成功提示并跳转到登录页

### 需求：修改密码
系统**必须**在个人中心提供修改密码功能，调用 `/login/modifypassword` 接口。

#### 场景：修改密码
- **当** 用户输入旧密码和新密码并提交
- **那么** POST 请求 `/login/modifypassword?OldPwd=xxx&NewPwd=xxx`
- **当** 返回成功
- **那么** 清除认证状态，重定向到登录页

### 需求：微信登录入口
如启用微信登录，系统**必须**提供微信扫码/跳转入口，调用 `/login/getopenid` 接口。

#### 场景：微信登录流程
- **当** 用户点击微信登录
- **那么** 跳转到微信授权页面
- **当** 授权成功回调携带 code
- **那么** 调用 `/login/getopenid?code=xxx` 获取 OpenId 完成登录

**交叉引用**：依赖 `frontend-data-layer`（API 层和状态管理）
