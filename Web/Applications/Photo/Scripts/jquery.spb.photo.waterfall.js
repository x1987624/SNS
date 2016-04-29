$(document).ready(function () {
    //瀑布流
    var $container = $('#spbThumbnailFalls');

    //翻页加数据
    $container.infinitescroll({
        navSelector: '#page-nav',    // selector for the paged navigation 
        nextSelector: '#page-nav a',  // selector for the NEXT link (to page 2)
        itemSelector: '.tnui-photo-waterfall'     // selector for all items you'll retrieve

    },
          function (newElements) {
              // hide new items while they are loading
              var $newElems = $(newElements).css({ opacity: 0 });
              // ensure that images load before adding to masonry layout
              $newElems.imagesLoaded(function () {
                  // show elems now they're ready
                  $newElems.animate({ opacity: 1 });
                  $container.masonry('appended', $newElems, true);
              });
          }
        );

    function loadData() {
        var url = $("#page-nav a").attr("href").replace("pageindex=2", "pageindex=1");
        $.get(url, function (data, status, xhr) {
            if (status === "success") {
                if (data.length > 0) {
                    $container.append(data);

                    //注册瀑布流插件并首次加载
                    $container.imagesLoaded(function () {
                        $container.masonry({
                            itemSelector: '.tnui-photo-waterfall',
                            columnWidth: function (containerWidth) {
                                return containerWidth / 4;
                            },
                            isResizable: true
                        });
                    });
                } else {
                    var noData = $("<div class='tn-box-content tn-widget-content tn-corner-bottom'><div class='tn-no-data'>暂无数据</div></div>");
                    $container.append(noData);
                }
            }
        });
    }

    //页面加载时加载初始化数据
    loadData();


    //加载图片上的分享，喜欢，评论操作按钮
    $("div[id^='ButtonView-']").livequery('mouseenter', function () {
        var photoId = $(this).data('photoid');
        var url = $(this).data('url');
        $.ajax({
            type: "get",
            url: url,
            data: { "photoId": photoId },
            success: function (msg) {
                $("#PhotoChannelButtonView-" + photoId).replaceWith(msg);
            }
        });

    });


    //鼠标移过显示大图脚本
    var photoWaterfallContainer = $("#spbThumbnailFalls");
    var t;
    $(".spb-photo-unit").livequery('mouseenter', function () {
        $(this).css("z-index", "1");
        var _this = $(this);
        var _photozoom = _this.find(".spb-photo-zoom");
        t = setTimeout(function () { _photozoom.fadeIn(200) }, 300);

        if (_this.offset().left > (photoWaterfallContainer.width() / 2 + photoWaterfallContainer.offset().left)) {
            _photozoom.css({ left: "auto", right: "-11px" });
        };

        _photozoom.die('mouseleave').livequery('mouseleave', function () {
            $(this).hide();
        }).die('mouseenter').livequery('mouseenter', function () {
            clearInterval(t);
        });
    }).livequery('mouseleave', function () {
        $(this).css("z-index", "auto");
        clearInterval(t);
    });

    //删除瀑布流中的照片
    $("a[id^='delete-waterfall-photo-']").livequery('click', function (e) {
        e.preventDefault();
        $this = $(this);
        art.dialog.confirm('您确定要删除该照片吗？', function () {
            $.post($this.attr("href"), function (data) {
                art.dialog.tips(data.MessageContent, 1.5, data.MessageType, function () {
                    $this.closest(".tnui-photo-waterfall").remove();
                });
                return false;
            });
        });
    });



});