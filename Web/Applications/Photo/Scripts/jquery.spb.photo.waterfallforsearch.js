$(document).ready(function () {
    //瀑布流
    var $container = $('#searchWaterFalls');

    //翻页加数据
    $container.infinitescroll({
        navSelector: '#page-nav',    // selector for the paged navigation 
        nextSelector: '#page-nav a',  // selector for the NEXT link (to page 2)
        itemSelector: '.tnui-photo-waterfallforsearch'     // selector for all items you'll retrieve
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
        var url = $("#page-nav a").attr("href").replace("pageIndex=2", "pageIndex=1");
        $.get(url, function (data, status, xhr) {
            if (status === "success") {
                if (data.length > 0) {
                    $container.append(data);

                    //注册瀑布流插件并首次加载
                    $container.imagesLoaded(function () {
                        $container.masonry({
                            itemSelector: '.tnui-photo-waterfallforsearch',
                            columnWidth: function (containerWidth) {
                                return containerWidth / 4;
                            },
                            isResizable: true
                        });
                    });
                } else {
                    var keyword = $("#keyword", $("#searchForm")).val();
                    if (keyword == "") {
                        var noData = $("<div class='tn-cue-tips tn-corner-all tn-message-box tn-border-gray tn-bg-gray'><a class='tn-icon tn-smallicon-cross tn-helper-right' href='#'></a><span class='tn-helper-left'><span class='tn-icon tn-icon-exclamation'></span></span><div class='tn-helper-flowfix'><strong>提示：</strong>请输入搜索条件</div></div>");

                        $container.append(noData);
                    } else {
                        var noData = $("<div class='tnc-search-noresults'><p class='tn-title'>很抱歉，没有找到符合您搜索条件的结果！</p><dl class='tn-support'><dt><strong>建议：</strong></dt><dd>请确保搜索文字拼写正确</dd><dd>请尝试使用其他关键词或使用近义词</dd><dd>请尝试使用含义更为宽泛的关键词</dd></dl></div>");
                        $container.append(noData);
                    }
                }
            }
        });
    }

    //页面加载时加载初始化数据
    loadData();

    //加载瀑布流图片上的分享，喜欢，评论操作按钮
    $("div[id^='ButtonView-']").livequery('mouseenter', function () {
        var photoId = $(this).data('photoid');
        var url = $(this).data('url');
        $.ajax({
            type: "get",
            url: url,
            data: { "photoId": photoId },
            success: function (msg) {
                $("#PhotoSearchButtonView-" + photoId).replaceWith(msg);
            }
        });

    });

    //鼠标移过显示大图脚本
    var photoWaterfallContainer = $("#searchWaterFalls");
    var t;

    $(".spb-photo-unit").livequery('mouseenter', function () {

        $(this).css("z-index", "100000");
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
        $(this).css("z-index", "1");
        clearInterval(t);
    });

});