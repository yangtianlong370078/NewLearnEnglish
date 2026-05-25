//默认为false,只有修改后才设置为true
var isupData = false;
//是否可以执行修改，避免重复提交
var isup = true;

//初始加载的
var oldMap = new Map();
//有变动的
var upMap = new Map();

//有变动的
var isupMap = new Map();

//更新数据
function upData() {
    
    if (isup) {
        isup = false;

        if (isupData) {
            // 新集合
            var newList = [];

            // 遍历第一个Map
            oldMap.forEach(function (value, key) {
                // 获取第二个Map中相同的键的值
                let value2 = upMap.get(key);

                // 比较两个值，如果不同则加入新集合
                if (value.zyno !== value2.zyno) {
                    newList.push({
                        id: key,
                        no: value2.zyno,
                        type:"zynumber"
                    });
                }
                if (value.yzno !== value2.yzno) {
                    newList.push({
                        id: key,
                        no: value2.yzno,
                        type: "yznumber"
                    });
                }
                if (value.txno !== value2.txno) {
                    newList.push({
                        id: key,
                        no: value2.txno,
                        type: "txnumber"
                    });
                }
                if (value.fyno !== value2.fyno) {
                    newList.push({
                        id: key,
                        no: value2.fyno,
                        type: "fynumber"
                    });
                }

            });
            //console.log(newList);

            //如果存在，才去更新
            if (newList.length > 0) {
                //去服务器更新
                var jwtToken = localStorage.getItem('jwtToken');

                $.ajax({
                    type: "post",
                    url: "/Home/updcno",
                    data: { data: JSON.stringify(newList) },
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader('Authorization', 'Bearer ' + jwtToken);
                    },
                    success: function (data) {

                        if (data.succss) {

                            console.log('更新了');
                            $(".upbs").hide();
                            //将新的集合替换老的集合
                            // 清空 oldMap
                            oldMap.clear();
                            // 深拷贝 upMap 到 oldMap
                            for (let [key, value] of upMap) {
                                oldMap.set(key, value);
                            }
                            //更新成功后重新设置为false
                            isupData = false;
                            //允许再次调用该方法
                            isup = true;
                        }
                        else {
                            czjieguo(data.msg, 1);
                        }
                    }
                })

            }
            else {
                isup = true;
                $(".upbs").hide();
            }

        }
    }
   

}



//切换页面执行
// 选择要观察的元素
var targetNode = document.getElementById('main');
// 配置观察器选项
var config = { childList: true };
// 当检测到变化时执行的回调函数
var callback = function (mutationsList, observer) {
    for (const mutation of mutationsList) {
        if (mutation.type === 'childList') {
            //console.log('节点添加或删除1');
            isup = true;
            upData();
        }

    }
};

// 创建观察器实例
var observer = new MutationObserver(callback);
// 开始观察目标节点
observer.observe(targetNode, config);

//页面关闭执行
window.addEventListener('beforeunload', function (event) {
// cleanupAudioMap();
    isbfaudio=1;
    isup = true;
    upData();
    // 这里可以执行一些清理工作，但请注意，某些操作可能不会被执行。
    // 浏览器可能会忽略耗时过长的处理。
   // event.preventDefault(); // 可选：阻止默认行为，某些情况下可能需要弹出确认对话框。
    // 注意：在某些浏览器中，直接弹出确认离开页面的对话框可能不被允许。
    // return '你确定要离开此页面吗？'; // 这行代码有时可以用来提示用户，但不是所有浏览器都支持。

    //console.log('页关闭了');
});


//页面隐藏执行
document.addEventListener('visibilitychange', function () {
    if (document.visibilityState === 'hidden') {
        isup = true;
       isbfaudio=1;
 if(dqaudio!=null){
          dqaudio.stop();
      }
//cshAudio();
// audYS.play();
      // cleanupAudioMap();
        upData();
        // 页面被隐藏时执行的代码
        //console.log('页面被隐藏了');
    } else {
        // 页面变为可见时执行的代码
        //console.log('页面变得可见了');



//cshAudio();
    isbfaudio=1;
    if (typeof dqaudio !== 'undefined' && dqaudio!=null){
          dqaudio.stop();
      }
     //解决移动端从页面切换回来没有声音的问题
    //audYS.play();
    }
});





