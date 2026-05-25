////[widget morris charts Javascript]

////Project:	EduAdmin - Responsive Admin Template
////Primary use:   Used only for the morris charts


//$(function () {
//    debugger
//    "use strict";

//    // bar chart
//    Morris.Bar({
//        element: 'bar-chart',
//        data: [{
//            y: '1\u6708',
//            a: 105,
//            b: 95,
//            c:2025
//        }, {
//            y: '2\u6708',
//            a: 80,
//            b: 70,
//            c: 2025
//        }, {
//            y: '3\u6708',
//            a: 55,
//            b: 45,
//            c: 2025
//        }, {
//            y: '4\u6708',
//            a: 80,
//            b: 70,
//            c: 2025
//        }, {
//            y: '5\u6708',
//            a: 55,
//            b: 45,
//            c: 2025
//        }, {
//            y: '6\u6708',
//            a: 80,
//            b: 70,
//            c: 2025
//        }, {
//            y: '7\u6708',
//            a: 105,
//            b: 95,
//            c: 2025
//        }, {
//            y: '8\u6708',
//            a: 105,
//            b: 95,
//            c: 2025
//        }, {
//            y: '9\u6708',
//            a: 105,
//            b: 95,
//            c: 2025
//        }, {
//            y: '10\u6708',
//            a: 105,
//            b: 95,
//            c: 2025
//        }, {
//            y: '11\u6708',
//            a: 105,
//            b: 95,
//            c: 2025
//        }, {
//            y: '12\u6708',
//            a: 105,
//            b: 95,
//            c: 2025
//        }],
//        xkey: 'y',
//        ykeys: ['a', 'b'],
//        labels: ['A', 'B'],
//        barColors: ['#04a08b', '#00baff'],
//        hideHover: 'auto',
//        gridLineColor: '#eef0f2',
//        resize: true,
//        parseTime: false,          // 禁用时间解析（重要！）
//        gridTextFamily: "'Microsoft YaHei', 'PingFang SC', 'STHeiti', sans-serif", // 中文字体
//        gridTextSize: 12,         // 字体大小

//        hoverCallback: function (index, options, content, row) {
//            let jg = Math.round(row.a / row.b * 100);


//            // 自定义HTML内容
//            return `
//                <div class='custom-tooltip'>
//                    <div class='tooltip-header'>${row.y} (${jg < 100 ? "\u672a\u5b8c\u6210" :"\u5df2\u5b8c\u6210"})</div>
//                    <table>
//                        <tr>
//                            <td>\u65b0\u5b66\u5355\u8bcd:</td>
//                            <td class='value'>${row.a}</td>
//                        </tr>
//                        <tr>
//                            <td>\u4efb\u52a1\u5355\u8bcd:</td>
//                            <td class='value'>${row.b}</td>
//                        </tr>
//                        <tr class='total'>
//                            <td>\u5b8c\u6210\u60c5\u51b5:</td>
//                            <td class='value'>${jg}%</td>
//                        </tr>
//                    </table>
                    
//                </div>
//            `;
//        }
  
//    });

//});