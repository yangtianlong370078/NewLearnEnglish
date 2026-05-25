//[calendar Javascript]

//Project:	EduAdmin - Responsive Admin Template
//Primary use:   Used only for the event calendar


!function ($) {
    "use strict";

    var CalendarApp = function () {
        this.$body = $("body")
        this.$calendar = $('#calendar'),
            this.$event = ('#external-events div.external-event'),
            this.$categoryForm = $('#add-new-events form'),
            this.$extEvents = $('#external-events'),
            this.$modal = $('#my-event'),
            this.$saveCategoryBtn = $('.save-category'),
            this.$calendarObj = null
    };


    /* on drop */
    CalendarApp.prototype.onDrop = function (eventObj, date) {
        var $this = this;
        // retrieve the dropped element's stored Event Object
        var originalEventObject = eventObj.data('eventObject');
        var $categoryClass = eventObj.attr('data-class');
        // we need to copy it, so that multiple events don't have a reference to the same object
        var copiedEventObject = $.extend({}, originalEventObject);
        // assign it the date that was reported
        copiedEventObject.start = date;
        if ($categoryClass)
            copiedEventObject['className'] = [$categoryClass];
        // render the event on the calendar
        $this.$calendar.fullCalendar('renderEvent', copiedEventObject, true);
        // is the "remove after drop" checkbox checked?
        if ($('#drop-remove').is(':checked')) {
            // if so, remove the element from the "Draggable Events" list
            eventObj.remove();
        }
    },
        /* on click on event */
        CalendarApp.prototype.onEventClick = function (calEvent, jsEvent, view) {


            var $this = this;
            var form = $("<form></form>");
            form.append("<label>Change event name</label>");
            form.append("<div class='input-group'><input class='form-control' type=text value='" + calEvent.title + "' /><span class='input-group-btn'><button type='submit' class='btn btn-success waves-effect waves-light'><i class='fa fa-check'></i> Save</button></span></div>");
            $this.$modal.modal({
                backdrop: 'static'
            });
            $this.$modal.find('.delete-event').show().end().find('.save-event').hide().end().find('.modal-body').empty().prepend(form).end().find('.delete-event').unbind('click').click(function () {
                $this.$calendarObj.fullCalendar('removeEvents', function (ev) {
                    return (ev._id == calEvent._id);
                });
                $this.$modal.modal('hide');
            });
            $this.$modal.find('form').on('submit', function () {
                calEvent.title = form.find("input[type=text]").val();
                $this.$calendarObj.fullCalendar('updateEvent', calEvent);
                $this.$modal.modal('hide');
                return false;
            });
        },
        /* on select */
        CalendarApp.prototype.onSelect = function (start, end, allDay) {
            var $this = this;
            $this.$modal.modal({
                backdrop: 'static'
            });
            var form = $("<form></form>");
            form.append("<div class='row'></div>");
            form.find(".row")
                .append("<div class='col-md-6'><div class='form-group'><label class='control-label'>Event Name</label><input class='form-control' placeholder='Insert Event Name' type='text' name='title'/></div></div>")
                .append("<div class='col-md-6'><div class='form-group'><label class='control-label'>Category</label><select class='form-control' name='category'></select></div></div>")
                .find("select[name='category']")
                .append("<option value='bg-danger'>Danger</option>")
                .append("<option value='bg-success'>Success</option>")
                .append("<option value='bg-purple'>Purple</option>")
                .append("<option value='bg-primary'>Primary</option>")
                .append("<option value='bg-pink'>Pink</option>")
                .append("<option value='bg-info'>Info</option>")
                .append("<option value='bg-warning'>Warning</option></div></div>");
            $this.$modal.find('.delete-event').hide().end().find('.save-event').show().end().find('.modal-body').empty().prepend(form).end().find('.save-event').unbind('click').click(function () {

                form.submit();
            });
            $this.$modal.find('form').on('submit', function () {
                var title = form.find("input[name='title']").val();
                var beginning = form.find("input[name='beginning']").val();
                var ending = form.find("input[name='ending']").val();
                var categoryClass = form.find("select[name='category'] option:checked").val();
                if (title !== null && title.length != 0) {
                    $this.$calendarObj.fullCalendar('renderEvent', {
                        title: title,
                        start: start,
                        end: end,
                        allDay: false,
                        className: categoryClass
                    }, true);
                    $this.$modal.modal('hide');
                }
                else {
                    alert('You have to give a title to your event');
                }
                return false;

            });
            $this.$calendarObj.fullCalendar('unselect');
        },
        CalendarApp.prototype.enableDrag = function () {
            //init events
            $(this.$event).each(function () {
                // create an Event Object (http://arshaw.com/fullcalendar/docs/event_data/Event_Object/)
                // it doesn't need to have a start or end
                var eventObject = {
                    title: $.trim($(this).text()) // use the element's text as the event title
                };
                // store the Event Object in the DOM element so we can get to it later
                $(this).data('eventObject', eventObject);
                // make the event draggable using jQuery UI
                $(this).draggable({
                    zIndex: 999999,
                    revert: true,      // will cause the event to go back to its
                    revertDuration: 0  //  original position after the drag
                });
            });
        }
    /* Initializing */
    CalendarApp.prototype.init = function (datas = [], task = null, date2 = new Date()) {

     


       // $(".fc-unselectable").hide();
        var istz = false;
        //重置组件
        if (this.$calendarObj) {
            
            //重置控件
            this.$calendarObj.fullCalendar('destroy');
            this.$calendarObj = null;
            istz = true;
        }
        this.enableDrag();

        //当前月份
        var dqdata = new Date();
        var dy = dqdata.getFullYear();
        var dm = dqdata.getMonth();
        var defaultEvents = [];


        for (var p = 0; p < datas.length; p++) {
            var data = datas[p];
            //任务数量
            var dividend = 0;


            if (data.task != null && data.task.count > 0) {
                dividend = data.task.count;
            }


            if (data.statisticsLearns.length > 0 || dividend > 0) {




                var d = 1;
                var m = data.month - 1;
                var y = data.year;

                var today = new Date($.now());




                // 创建一个新的Date对象来保存当前月份的最后一天  
                let newdate = new Date(data.year, data.month - 1, 1);

                // 设置newdate为下个月的第一天（月份从0开始，所以加1）
                newdate.setMonth(newdate.getMonth() + 1);

                // 设置日期为0以获取当前月份的最后一天  
                newdate.setDate(0);

                // 此时newdate是当前月份的最后一天  
                var lastDayOfMonth = newdate.getDate();

                
                //获取周六周日
                //用于计算周六周日
                let zlzrdate = new Date(data.year, data.month - 1, 1);
                //保存周日周日
                var weekendDays = []; 
                //1只计算周六，2只计算周日，3，周六周日都计算
                var xxtype = data.task.weekend;

                if (xxtype > 0) {
                    while (zlzrdate.getMonth() === data.month - 1) {
                        let dayOfWeek = zlzrdate.getDay();
                        // 0是周日，6是周六  
                        if ((xxtype == 1 && dayOfWeek === 6) || (xxtype == 2 && dayOfWeek === 0) || (xxtype == 3 && (dayOfWeek === 0 || dayOfWeek === 6))) {
                            weekendDays.push(zlzrdate.getDate()); // 只添加“日”部分  
                        }
                        zlzrdate.setDate(zlzrdate.getDate() + 1);
                    }  
                }
               
                

                var quotient = Math.floor(dividend / (lastDayOfMonth - weekendDays.length));
                var remainder = dividend % (lastDayOfMonth - weekendDays.length);





                if (dy == y && dm == m) {
                    d = dqdata.getDate();
                }


                m = m + 1;
                for (var i = 1; i <= lastDayOfMonth; i++) {


                    let result = data.statisticsLearns.find(item => item.year === y && item.month === m && item.day === i);
                    //学习数量
                    let okcount = 0;
                    if (result != undefined) {

                        okcount = result.count;
                    }

                    
                    let isxx = weekendDays.includes(i);
                    if (isxx) {
                        remainder++;
                    }
                    let rwcount = isxx ? 0 : (quotient + (remainder >= i ? 1 : 0));
                    
                    let start = y + '-' + (m < 10 ? '0' + m : '' + m) + '-' + (i < 10 ? '0' + i : '' + i);
                    let className = 'bg-lighter';
                    let title = okcount > 0 ? (rwcount + '/' + okcount) : rwcount;


                    if (dividend == 0) {
                        title = okcount > 0 ? okcount : '';
                        className = okcount > 0 ? 'bg-info' : '';
                    }


                    var isxydate = false;
                    if (dy > y) {
                        isxydate = true;
                    } else if (dy == y && dm > (m - 1)) {
                        isxydate = true;
                    } else if (dy == y && dm == (m - 1) && d >= i) {
                        isxydate = true;
                    }

                    if (isxydate && dividend > 0) {
                        className = rwcount <= okcount ? 'bg-success' : okcount == 0 ? 'bg-danger' : 'bg-warning'
                    }

                    if (className != '') {
                        defaultEvents.push({
                            title: title,
                            start: start,
                            end: start,
                            className: className
                        });
                    }

                }
            }
        }

        var defaultEvents2 = [{
            title: 'Released Ample Admin!',
            start: '2017-08-08',
            end: '2017-08-08',
            className: 'bg-info'
        }, {
            title: '这是什么',
            start: today,
            end: today,
            className: 'bg-danger'
        }, {
            title: '完成：8/4',
            start: '2024-06-05',
            end: '2024-6-05',
            className: 'bg-lighter'
        }, {
            title: '完成：8/9',
            start: '2024-06-06',
            end: '2024-06-06',
            className: 'bg-info'
        }, {
            title: 'This is your birthday',
            start: '2017-09-08',
            end: '2017-09-08',
            className: 'bg-info'
        },
        {
            title: 'Hanns birthday',
            start: '2017-10-08',
            end: '2017-10-08',
            className: 'bg-danger'
        }, {
            title: '就是这个',
            start: new Date($.now() + 784800000),
            className: 'bg-success'
        }];

        var $this = this;
        $this.$calendarObj = $this.$calendar.fullCalendar({
            slotDuration: '00:15:00', /* If we want to split day time each 15minutes */
            minTime: '08:00:00',
            maxTime: '19:00:00',
            defaultView: 'month',
            handleWindowResize: true,

            header: {
                //left: 'prev,next today', //显示今天按钮
                left: 'prev,next',
                center: 'title',
                right: ''
            },
            events: defaultEvents,
            editable: false,
            droppable: false, // this allows things to be dropped onto the calendar !!!
            eventLimit: false, // allow "more" link when too many events
            selectable: false,
            drop: function (date) {
                $this.onDrop($(this), date);
            },
            select: function (start, end, allDay) {
                $this.onSelect(start, end, allDay);
            },
            eventClick: function (calEvent, jsEvent, view) {
                $this.onEventClick(calEvent, jsEvent, view);
            }
        });

        //on new event
        this.$saveCategoryBtn.on('click', function () {
            var categoryName = $this.$categoryForm.find("input[name='category-name']").val();
            var categoryColor = $this.$categoryForm.find("select[name='category-color']").val();
            if (categoryName !== null && categoryName.length != 0) {
                $this.$extEvents.append('<div class="m-15 external-event bg-' + categoryColor + '" data-class="bg-' + categoryColor + '" style="position: relative;"><i class="fa fa-hand-o-right"></i>' + categoryName + '</div>')
                $this.enableDrag();
            }

        });
        
        $(".fc-view-container").addClass("boxnc").addClass("rwrqys");

        $('.fc-content-skeleton thead td').each(function (index) {
            
            if ($(this).hasClass('fc-other-month')) {
                var isbh = $('.fc-content-skeleton table tbody tr td:eq(' + index + ')').hasClass('fc-event-container')
                if (isbh) {
                    $('.fc-content-skeleton table tbody tr td:eq(' + index + ')').addClass('btmxiaoguo');
                }
            }
        });

        $('.fc-left .btn-group button').on('click', function (event) {
            
            var ty = $(this).attr("aria-label");

            var rq = $(".fc-center").text();
           // var newrq = parseCustomDate(rq, ty == 'next' ? 1 : -1);
            let newrq = parseCustomDate(rq, 0);
           
           // window.CalendarApp.$calendarObj.fullCalendar('gotoDate', newrq);
            settjsuju(newrq);

            //setTimeout(function () {
            //    settjsuju(newrq);
            //}, 0);
            
           
            

            // this.$calendarObj.gotoDate(date);
            // 阻止默认事件或冒泡，如果需要的话  
            // event.preventDefault();  
            // event.stopPropagation();  
        });

         if (istz) {

         //this.$calendarObj.fullCalendar('prev');
         //this.$calendarObj.gotoDate(date);
         //this.$calendarObj.gotoDate(date);

        //这个是有用的
         // this.$calendarObj.fullCalendar('gotoDate', date);
            // debugger
            // let newrq = parseCustomDate(rq, 0);
            // settjsuju(newrq);
          }

        //将操控按钮移动到指定元素内appendTo为最后面，prependTo为最前面
        //$('.fc-header-toolbar').appendTo('.datamun');

        $('.fc-header-toolbar').prependTo('.datamun');

        
        //绑定日期控件
        $(".fc-center H2").addClass("jeindiv").addClass("btn").addClass("btn-primary");//.addClass("divmore");
        $(".jeindiv").attr("placeholder", "YYYY年MM月");

        
        var divel = document.querySelectorAll(".jeindiv");

        for (var i = 0; i < divel.length; i++) {
            var divmat = divel[i].getAttribute("placeholder");
            jeDate(divel[i], {
                format: divmat
            });
        }
      
        $('.fc-center').insertAfter('.fc-prev-button.fc-corner-left');
        
        settjsuju(dqdata);

    },

        //init CalendarApp
        $.CalendarApp = new CalendarApp, $.CalendarApp.Constructor = CalendarApp
    // 将CalendarApp暴露给全局作用域
    window.CalendarApp = new CalendarApp();
}(window.jQuery);

function updateCalendarWithData(newData, task, newDate) {

    "use strict";
    if (window.CalendarApp) {
        window.CalendarApp.init(newData, task, newDate);
    }
}

function updateCalendarWithDataTwo(newData) {

    "use strict";
    if (window.CalendarApp) {
        window.CalendarApp.init(newData);
    }
}


function qiehuanshijian(date) {
    this.$calendarObj.fullCalendar('gotoDate', date);
}