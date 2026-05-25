//[c3 charts Javascript]

//Project:	EduAdmin - Responsive Admin Template
//Primary use:   Used only for the morris charts


$(function () {
    "use strict";
	var a = c3.generate({
        bindto: "#data-color",
        size: { height: 350},
		data: {
			x: "x",
			columns: [
                ['x', " 1月", "2月", "3月", "4月", "5月", "6月"],
                ['data1', 30, 20, 50, 40, 60, 50],
				['data2', 200, 130, 90, 240, 130, 220]
            ],
            type: "bar",
            colors: { data1: "#38649f", data2: "#389f99" }
        },
        groups: [
            ["data1", "data2"]
        ],
		axis: { x: { type: "category" } },
        grid: { y: { show: !0 } }
    });
  });