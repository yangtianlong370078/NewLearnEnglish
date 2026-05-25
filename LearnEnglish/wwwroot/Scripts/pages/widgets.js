//[widget charts Javascript]


$(document).ready(function () {
    "use strict";

    let totalCount = 0; 
    let chart;  

    var options = {
        series: [0, 0],
        chart: {
            height: 325,
            type: 'radialBar',
        },
        colors: ["#f64e60", "#1bc5bd"],
        stroke: {
            lineCap: "round",
        },
        plotOptions: {
            radialBar: {
                dataLabels: {
                    name: {
                        show: true,
                    },
                    value: {
                        fontSize: '30px',
                    },
                    total: {
                        show: true,
                        label: '学习单词',
                        formatter: function (w) {
                            
                            // By default this function returns the average of all series. The below is just an example to show the use of custom formatter function
                         var xuexi =   w.config.series[0];
                            return totalCount;
                        }
                    }
                }
            }
        },
        labels: ['时间进度', '学习进度'],
    };

    chart = new ApexCharts(document.querySelector("#revenue7"), options);
    chart.render();

    // 公开一个可以在外部调用的函数  
    window.updateSeriesAndTotal = (function () {
        // 这里是闭包，可以访问totalCount和chart  
        return function (newSeries, newTotal) {
            totalCount = newTotal;

            //设置宽度
            chart.updateOptions({
                plotOptions: {
                    radialBar: {
                        hollow: {
                            size: newSeries.length == 2 ? 50 : 67,
                        },
                    },
                },
            });
            
            chart.updateSeries(newSeries);

           
        };
    })();  
   
}); 