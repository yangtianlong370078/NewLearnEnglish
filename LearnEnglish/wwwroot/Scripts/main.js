/// <reference path="jquery-1.8.2.min.js" />
/// <reference path="../Content/layer/layer.min.js" />
var hasDone = true;
$(document).ready(function () {
    $(window).unbind("hashchange");
    window.onload = processHash;
    window.onhashchange = processHash;

    $.ajaxSetup({
        cache: false
    });
});


function showpage(page) {
    
    if (page != "main") {
        $("#main").hide();
    }
    if (page != "dqhome") {
        $("#dqhome").hide();
    }
    if (page != "dqCategoryList") {
        $("#dqCategoryList").hide();
    }
    if (page != "dqMyCategoryList") {
        $("#dqMyCategoryList").hide();
    }
    if (page != "dqExamList") {
        $("#dqExamList").hide();
    }
    if (page != "dqMy") {
        $("#dqMy").hide();
    }
    $("#" + page).show();

    
}
function processHash() {
    
    var hashStr = location.hash.replace("#", "");
    if (hashStr.length > 0) {
        if (!hasDone) {
            hasDone = true;
        } else {

            //$('#main').empty().append("<div style=\"text-align: center;\"><img src=\"../Content/img/sunjing_loading2.gif\" /></div>");
            //$('#main').load(hashStr, function () {
            //    hasDone = true;
            //});


            if (hashStr.indexOf('Home/Home') !== -1) {
                if ($('#dqhome').children().length == 0) {


                    showpage("dqhome");
                    requestUrlHtml("/Home/Home", "", "dqhome");
                }
            }
            else if (hashStr.indexOf('Course/CategoryList') !== -1) {
               
                    showpage("main");
                    requestUrlHtml(hashStr, "", "main");
            }
            else if (hashStr.indexOf('Course/MyCategoryList') !== -1) {
                if ($('#dqMyCategoryList').children().length == 0) {
                    showpage("dqMyCategoryList");
                    requestUrlHtml(hashStr, "", "dqMyCategoryList");
                }
            }
            else if (hashStr.indexOf('Exam/ExamList') !== -1) {
                if ($('#dqExamList').children().length == 0) {
                    showpage("dqExamList");
                    requestUrlHtml("/Exam/ExamList", "", "dqExamList");
                }
            }
            else {
                showpage("main"); 
                requestUrlHtml(hashStr, "", "main");
            }


        }
    }
}

function requestUrl(url, data, container) {
    hasDone = false;
    var index = layer.load(layerloading('正在加载... ...', 10));
    $("#" + container).load(url, data, function () {
        layer.close(index);
        if (container == "main") {
           // location.hash = "#" + url;
        }
    });
}
function showLayer(url, width, height, title) {
    if (width == undefined) {
        width = "400px";
    } else {
        width = "" + width + "px";
    }

    if (height == undefined) {
        height = "400px";
    } else {
        height = "" + height + "px";
    }

    $.layer({
        type: 2,
        title: title,
        maxmin: false,
        shadeClose: true, //开启点击遮罩关闭层
        area: [width, height],
        offset: ['10%', ''],
        iframe: {
            src: url
        }
    });
}
//点击层外，隐藏这个层。由于层内的事件停止了冒泡，所以不会触发这个事件  
$(document).click(function (e) {
    if (e.target) {
        if (e.target.localName == "i" || e.target.localName == "a" || e.target.localName == "input" || e.target.localName == "button") {
        } else {
            $(document).find("div.needClose").last().remove();
        }
    }
});
function logout() {
    layer.confirm("你确定要退出吗", function () {
        $.ajax({
            url: "/Default/Logout",
            dataType: "text",
            cache: false,
            type: "POST",
            success: function (d) {
                window.location.href = d.substr(1, d.length - 2);
            }, error: function (e) {
                layer.msg(e);
            }
        });
    }, "温馨提示");
}

function requestData(url, data){
 return new Promise((resolve) => {
// 获取存储的JWT Token，这里假设Token存在localStorage中以"jwtToken"为键名存储
         var jwtToken = localStorage.getItem('jwtToken');
                 // 使用ajax设置自定义headers包含Token
         $.ajax({
             url: url,
             type: 'post',
             data: data,
            // contentType: 'application/json', // 设置内容类型
             dataType: 'json', // 期望的响应类型
             beforeSend: function(xhr) {
                 xhr.setRequestHeader('Authorization', 'Bearer ' + jwtToken);
             }
         })
             .done(function (data, textStatus, jqXHR) {
             // 更清晰的状态码检查
                 if (jqXHR.status >= 200 && jqXHR.status < 300) {
                 
                 resolve(data);
             } else {
                 resolve(null);
             }
         })
             .fail(function (xhr, textStatus, errorThrown) {
             // resolve(null);
                 if (xhr.status === 401) {  
                            var baseUrl = window.location.protocol + "//" + window.location.host+"/Login/Login";   
                             window.location.href = baseUrl;  
                        
                 } else if (xhr.status === 410) {

                     var baseUrl = window.location.protocol + "//" + window.location.host + "/Login/Login";
                     window.location.href = baseUrl;

                 } else if (xhr.status === 409) {

                     var baseUrl = window.location.protocol + "//" + window.location.host + "/Error/UserError";
                     window.location.href = baseUrl;

                 } else {  
                         // 处理其他类型的错误  
                        // rejectWithError(xhr.status, textStatus);  
                     }  

         });

    });
}

/*
    自定义加载
*/
function requestUrlHtml(url, data, container) {
    //layer.closeAll();
    return new Promise((resolve) => {
   
           var dom = $("#" + container);
           $(dom).html("");
           $(dom).append("<div class=\"spinner\"></div>");
           hasDone = false;

          // 获取存储的JWT Token，这里假设Token存在localStorage中以"jwtToken"为键名存储
            var jwtToken = localStorage.getItem('jwtToken');
                 // 使用ajax设置自定义headers包含Token
           $.ajax({
             url: url,
             type: 'GET',
             data: data,
             beforeSend: function(xhr) {
                 xhr.setRequestHeader('Authorization', 'Bearer ' + jwtToken);
             }
         })
         .done(function(data, textStatus, jqXHR) {
             // 更清晰的状态码检查
             if (jqXHR.status >= 200 && jqXHR.status < 300) {
                 const containerElement = $("#" + container);
                 if (container === "main") {
                  //   location.hash = "#" + url;
                 }
         
                 if (data.indexOf('RequireLogon') !== -1) {
                     try {
                         const obj = JSON.parse(data);
                         const location = decodeURIComponent(obj.Result.split('?')[0]) + "?fromurl=" + encodeURIComponent(window.location.href);
                         containerElement.html(`<div style='text-align: center;padding:30px;'>您的登录已失效，请点击<a style='color:blue;' href='${location}'>重新登录</a></div>`);
                     } catch (e) {
                         console.error("解析JSON失败:", e);
                     }
                 } else {
                     containerElement.html(data);
                 }

                 resolve(true);
             } else {
                 console.warn("非预期的HTTP状态码:", jqXHR.status);
                 resolve(false);
             }
         })
         .fail(function(xhr, textStatus, errorThrown) {
         
         if (xhr.status === 401) {  
               
                
                    var baseUrl = window.location.protocol + "//" + window.location.host+"/Login/Login";   
                     window.location.href = baseUrl;  
                
             } else if (xhr.status === 410) {
         
                 
                 var baseUrl = window.location.protocol + "//" + window.location.host + "/Login/Login";
                 window.location.href = baseUrl;
         
             } else if (xhr.status === 409) {
         
                 
                 var baseUrl = window.location.protocol + "//" + window.location.host + "/Error/UserError";
                 window.location.href = baseUrl;
         
             } else {  
                 // 处理其他类型的错误  
                 rejectWithError(xhr.status, textStatus);  
             }  
         
            
         
         });
    });
}


/*
    后台加载下一页数据
*/
function BackLoadNext(url, data, backlist) {
    $.ajax({
        type: "POST",
        url: url,
        data: data,
        success: function (data) {
            $("#" + backlist).empty().append(data);
            $(".nextcolor").eq(0).css("color", "#51A351");
        }
    });
}


/*
    pageIndex：目标页
    url：url
    div：目标页数据div
    data：目标页参数
    hid_div：目标页下一页数据div
    data：目标页下一页参数
*/
function LoadingNext(pageIndex, url, div, data, hid_div, nextdata) {
    pageIndex = pageIndex == undefined ? 1 : pageIndex;
    var activeIndex = $("#" + div + " .mvc_page_fenye").children(".active").children("a").text();
    var hid_activeIndex = $("#" + hid_div + " .mvc_page_fenye").children(".active").children("a").text();
    //其它页
    if (parseInt(activeIndex) + 1 != pageIndex) {
        requestUrlHtml(url, data, div);
    }
    //下一页预加载
    if (parseInt(activeIndex) + 1 == pageIndex && parseInt(activeIndex) + 1 == parseInt(hid_activeIndex)) {
        $("#" + div).empty().append($("#" + hid_div).html());
    }
    //下一页预加载
    if (parseInt(activeIndex) + 1 == pageIndex && parseInt(activeIndex) + 1 != parseInt(hid_activeIndex)) {
        requestUrlHtml(url, data, div);
    }
    BackLoadNext(url, nextdata, hid_div);
}

/*
    倒计时
*/
function layerloading(content, time) {
    var html = "<span id='layer-loadingTime' style='margin-left: -28px;font-size: 15px;'></span><span id='layer-loadingName' style='margin-left:17px;' >" + content + "</span><script type='text/javascript'>var i = " + time + ";function change() {i--; $('#layer-loadingTime').text(i);if(i>6&&i<=10){$('#layer-loadingTime').css('color','green')};if(i>3&&i<=6){$('#layer-loadingTime').css('color','#FF7C2F')};if(i>0&&i<=3){$('#layer-loadingTime').css('color','red')} if (i == 0){$('#layer-loadingTime').text('0').css('color','#fff');}else{setTimeout('change()', 1000);}}change();</script>";
    return html;
}
