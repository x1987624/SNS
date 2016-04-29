(function ($) {

    //绑定jquery入口
    $.PhotoViewer = function (photoId) {
        new PhotoViewer(photoId).init();
    };

    /*
    * 照片浏览器类
    */
    var PhotoViewer = function (photoId) {
        this.photoId = photoId;
    };

    PhotoViewer.prototype = {
        init: function () {
            var _this = this;

            //点击提交按钮
            $(".tn-submit").die().live("click", function () {
                $(this).parents("form:first").submit();
                return false;
            });

            //如果页面中已经存在了圈人框，那么删除之前的
            if ($("div[id='labelphoto-select']").length > 1)
                $("div[id='labelphoto-select']:last").remove();

            //管理详细页面的时候。将圈人回放到详细页面中。让他随详细页面关闭
            $("#closeViewer").click(function () {
                $("#labelphoto-select").hide().appendTo("div[id^='photo-viewer-']");
            });

            //圈人的时候关闭按钮点击时
            $("#cancelButton", $("div[id='labelphoto-select']")).click(function () {
                _this.exitLabelPhoto();
                _this.addRingFrame();
            });

            _this.photoViewer = $("#photo-viewer-" + _this.photoId); //照片浏览器容器dom对象
            _this.img = $("#photoImage-" + _this.photoId, _this.photoViewer);        //照片dom对象
            _this.photos = _this.photoViewer.data("photos");        //相册内所有照片的列表
            _this.index = 0;                                        //当前图片在相册图片中的数组下标
            _this.indexFrom = 0;                                    //当前缩略图分页的数组下标起始位置
            _this.indexTo = _this.photos.length;                    //当前缩略图分页的数组下标结束位置
            _this.pageIndex = 1;                                    //图片缩略图列表的页码
            _this.horizontalDirection = true;                       //图片是否处于水平方向
            _this.scaling = 1;                                      //缩放比例
            _this.photolabelslink = _this.photoViewer.attr("photolabelslink");  //圈人信息的连接

            //IE6下保持详细显示页面跟随主页面
            var isIE6 = $.browser.msie && $.browser.version == 6;
            if (isIE6) {
                $(window).scroll(function () {
                    _this.photoViewer.parents("div.aui_state_noTitle").offset({ top: $(window).scrollTop() - 5, left: 0 });
                });
                $(window).scroll();
            }

            _this.isLoadingLabel = false;

            //定位当前照片在所有照片中的位置
            for (var i = 0; i < _this.photos.length; i++) {
                if (_this.photos[i].photoId == _this.photoId) {
                    _this.index = i;
                    break;
                }
            }

            //缩放窗口显示区域
            _this.resizeWindow();

            //以预加载的方式显示照片
            _this.loadPhoto();

            //绑定窗口缩放事件
            $(window).resize(function () {
                _this.resizeWindow();
            });

            //绑定图片向左旋转点击事件
            $(".tn-icon-rotate-left", _this.photoViewer).click(function () {
                //如果正在播放，则停止播放
                _this.stopPlayPhoto();

                _this.rotatePhoto(-90);
            });

            //绑定图片向右旋转点击事件
            $(".tn-icon-rotate-right", _this.photoViewer).click(function () {
                //如果正在播放，则停止播放
                _this.stopPlayPhoto();

                _this.rotatePhoto(90);
            });

            //绑定侧边栏显示和隐藏按钮点击事件
            $(".tn-side-switch", _this.photoViewer).click(function () {
                _this.toggleSide();
            });

            //绑定“上一张”链接点击事件
            $(".tn-body .tn-page-prev", _this.photoViewer).click(function () {
                //如果正在播放，则停止播放
                _this.stopPlayPhoto();

                _this.loadPhotoPrev();
            });

            //绑定“下一张”链接点击事件
            $(".tn-body .tn-page-next", _this.photoViewer).click(function () {
                //如果正在播放，则停止播放
                _this.stopPlayPhoto();

                _this.loadPhotoNext();
            });

            //绑定键盘事件
            $(document).keydown(function (e) {

                //如果正在播放，则停止播放
                _this.stopPlayPhoto();

                if (e.keyCode == 37) {          //向左翻页-上一张
                    e.preventDefault();
                    _this.loadPhotoPrev();
                } else if (e.keyCode == 39) {   //向右翻页-下一张
                    e.preventDefault();
                    _this.loadPhotoNext();
                } else if (e.keyCode == 27) {
                    $(document).unbind("keydown", arguments.callee);
                    $("#labelphoto-select").hide().appendTo("div[id^='photo-viewer-']");
                    var list = art.dialog.list;
                    for (var i in list) {
                        list[i].close();
                    };
                }
            });

            //绑定缩略图点击事件
            $(".tn-thumb-list li", _this.photoViewer).live("click", function () {
                //如果正在播放，则停止播放
                _this.stopPlayPhoto();

                _this.index = $(this).data("index");
                _this.loadPhoto();
            });

            //绑定缩略图向前翻页点击事件
            $(".tn-foot .tn-page-prev", _this.photoViewer).click(function () {
                //如果正在播放，则停止播放
                _this.stopPlayPhoto();

                if (_this.pageIndex > 1) {
                    _this.index = _this.index - _this.pageSize;

                    _this.loadPhoto();
                } else {
                    art.dialog.tips('已经是第一页了', 1.5, 0);
                }
            });

            //绑定缩略图向后翻页点击事件
            $(".tn-foot .tn-page-next", _this.photoViewer).click(function () {
                //如果正在播放，则停止播放
                _this.stopPlayPhoto();

                if (_this.pageIndex < Math.ceil(_this.photos.length / _this.pageSize)) {
                    _this.index = _this.index + _this.pageSize;
                    if (_this.index >= _this.photos.length) {
                        _this.index = _this.photos.length - 1;
                    }

                    _this.loadPhoto();
                } else {
                    art.dialog.tips('已经是最后一页了', 1.5, 0);
                }
            });

            //绑定缩略图幻灯播放按钮点击事件            
            $("#playPhoto", _this.photoViewer).die().live("click", function () {
                if ($(this).find(".tn-button-text:first").text().indexOf("播放") >= 0) {
                    _this.startPlayPhoto();
                } else {
                    _this.stopPlayPhoto();
                }
            });

            //绑定“圈人”按钮点击事件            
            $("[id='labelPhoto']", _this.photoViewer).live("click", function () {
                _this.labelPhoto();
            });

            //删除圈人
            $("a[name='delPhotoLabel']", _this.photoViewer).die().live("click", function () {
                _this.deletePhotoLabel($(this));
            });

            //右侧的圈人信息鼠标移入移除方法
            $("li[id^='labelUser-']", _this.photoViewer).die("mouseenter").live("mouseenter", function () {
                $("#photoLabel-" + $(this).attr("name") + " div.tn-photo-note-circle").mouseenter();
            }).die("mouseleave").live("mouseleave", function () {
                $("#photoLabel-" + $(this).attr("name") + " div.tn-photo-note-circle").mouseleave();
            });

            //右侧内容的收放
            $("[id^='nexttoggle-']", _this.photoViewer).die().live("click", function (e) {
                e.preventDefault();
                $(this).parents("[name='parent']").find("[name='nexttoggle']").toggle();
                _this.resizeComment();
                $("a[name='toggleicon']", $(this)).toggleClass("tn-smallicon-triangle-up");
                $("a[name='toggleicon']", $(this)).toggleClass("tn-smallicon-triangle-down");
            });

            //编辑描述的form提交绑定事件
            $("#formSetDescription", _this.photoViewer).die().live("submit", function (e) {
                var _this = this;
                e.preventDefault();
                $(this).ajaxSubmit({
                    success: function (data) {
                        $(_this).parent(".tn-editor:first").toggle();
                        $(_this).parent(".tn-editor:first").prev(".tn-photo-description").find("span").html(data.MessageContent);
                        $(_this).parent(".tn-editor:first").prev(".tn-photo-description").toggle();
                    }
                });
            });

            //编辑描述的时候点击取消按钮
            $("#editDescriptionCancel", _this.photoViewer).die().live("click", function () {
                $(this).parents(".tn-editor:first").toggle();
                $(this).parents(".tn-editor:first").prev(".tn-photo-description").toggle();
            });

            //点击编辑描述按钮时
            $("a[id='photoDescriptionWrite']", _this.photoViewer).die().live("click", function (e) {
                e.preventDefault();
                $(this).parents(".tn-photo-description:first").toggle();
                $(this).parents(".tn-photo-description:first").next(".tn-editor").toggle();
            });

            //圈人的收放
            $("[id='toggle-PhotoLabel']", _this.photoViewer).die().live("click", function () {
                _this.addSideRingFrame();
            });

            //右侧编辑图片标签
            $("a[name='PhotoTagEdit']", _this.photoViewer).die().live("click", function () {
                $("a.tn-tag,a[name='PhotoTagEdit'],div[name='PhotoTagEdit'],a[name='showMorePhotoTag']", _this.photoViewer).toggle();
            });

            //右侧编辑标签的的表单提交
            $("#detailPhotoTagForm", _this.photoViewer).die().live("submit", function (e) {
                e.preventDefault();
                $(this).ajaxSubmit({
                    success: function (data) {
                        if (data.MessageType == 1) {
                            _this.addPhotoTags();
                        } else {
                            art.dialog.tips(data.MessageContent, 1.5, data.MessageType);
                        }
                    }
                });
            });

            //右侧编辑标签。点击取消的时候
            $("#PhotoTagEditCancel", _this.photoViewer).die().live("click", function () {
                _this.addPhotoTags();
            });

            //右侧标签的收起与展示
            $("a[name='showMorePhotoTag']", _this.photoViewer).die().live("click", function () {
                _this.toggleSideTag();
            });

            //右侧操作按钮
            $("#EditPhoto", _this.photoViewer).die().live("click", function () {
                _this.togglePhotoOperating();
            });

            //右下角的异步请求
            $("a[id^='ajaxJsonMessageData-']").die().live("click", function (e) {
                e.preventDefault();
                $.post($(this).attr("href"), function (data) {
                    art.dialog.tips(data.MessageContent, 1.5, data.MessageType);
                    _this.addFootPhotoDetail();
                });
            });

            //删除方法
            $("#ajaxJson-Del").die().live("click", function (e) {
                e.preventDefault();
                var $this = $(this);
                art.dialog.confirm('您确认要删除吗？', function () {
                    $.post($this.attr("href"), function (data) {
                        if (data.MessageType == 1) {
                            window.location.href = data.MessageContent;
                        } else {
                            art.dialog.tips(data.MessageContent, 1.5, data.MessageType);
                        }
                    });
                });
            });
        },

        /*
        *重新计算评论框外围的大小
        */
        resizeComment: function () {
            var _this = this;

            var $photoSide = $("#DetailPhotoSide", _this.photoViewer);
            var $comment = $(".tn-comment-area", $photoSide);
            var $commentContainer = $comment.find("[name = 'nexttoggle']");

            if ($commentContainer.is(":hidden")) {
                return;
            }


            var height = 0;
            $photoSide.children().each(function () {
                if (!$(this).is(".tn-comment-area")) {
                    height += $(this).height() + 21;
                }
            });

            var commentHeight = ($photoSide.parent().height() - height);
            commentHeight = commentHeight > 226 ? commentHeight : 226;

            $commentContainer.height(commentHeight - 16);
        },

        /*
        * 缩放窗口显示区域
        */
        resizeWindow: function () {
            var _this = this;

            var windowHeight = $(window).height(); //_this.photoViewer.height();
            var mainHeight = windowHeight - 140;

            $(".tn-body", _this.photoViewer).width($(document).width() - 20);

            $(".tn-body", _this.photoViewer).height(mainHeight);
            $(".tn-photo-inner", _this.photoViewer).height(mainHeight - 2);
            $(".tn-photo-container", _this.photoViewer).height(mainHeight - 22);

            _this.resizePhoto();

            //计算缩略图列表的分页大小及页码
            _this.pageSize = Math.floor($(".tn-thumb-list ul", _this.photoViewer).width() / 85);
            _this.pageIndex = Math.ceil((_this.index + 1) / _this.pageSize);

            //加载缩略图
            _this.loadThumbnails();

            _this.showUserSelect();

            _this.resizeComment();
        },

        /*
        * 缩放照片，并且使其自适应显示区域大小
        */
        resizePhoto: function () {
            var _this = this;
            _this.exitLabelPhoto();
            _this.removeRingFrame();

            var photoContainer = $(".tn-photo-container", _this.photoViewer);
            var containerWidth = photoContainer.width() - 40;    //图片显示区域的最大宽度=父容器的宽度-40
            var containerHeight = photoContainer.height() - 40;  //图片显示区域的最大高度=父容器的高度-40

            var imgWidth = _this.img.data("width") || _this.img.width();
            var imgHeight = _this.img.data("height") || _this.img.height();

            var imgRatio = imgWidth / imgHeight;

            //根据图片旋转后的水平或垂直方向计算缩放大小
            if (this.horizontalDirection) {
                var containerRatio = containerWidth / containerHeight;

                if (containerWidth >= imgWidth && containerHeight >= imgHeight) {
                    _this.img.width(imgWidth);
                    _this.img.height(imgHeight);

                    //设定缩放比例
                    _this.scaling = 1;
                } else {
                    if (imgRatio >= containerRatio) {
                        //设定缩放比例
                        _this.scaling = containerWidth / imgWidth;

                        _this.img.width(containerWidth);
                        _this.img.height(containerWidth / imgRatio);
                    } else {
                        //设定缩放比例
                        _this.scaling = containerHeight / imgHeight;

                        _this.img.width(containerHeight * imgRatio);
                        _this.img.height(containerHeight);
                    }
                }
            } else {
                var containerRatio = containerHeight / containerWidth;

                if (containerHeight >= imgWidth && containerWidth >= imgHeight && containerHeight >= imgHeight && containerWidth >= imgWidth) {
                    //设定缩放比例
                    _this.scaling = 1;

                    _this.img.width(imgWidth);
                    _this.img.height(imgHeight);
                } else {
                    var minContainer = containerHeight > containerWidth ? containerWidth : containerHeight;
                    if (imgRatio >= containerRatio && minContainer / imgRatio < minContainer) {
                        //设定缩放比例
                        _this.scaling = minContainer / imgWidth;

                        _this.img.width(minContainer);
                        _this.img.height(minContainer / imgRatio);
                    } else {
                        //设定缩放比例
                        _this.scaling = minContainer / imgHeight;

                        _this.img.width(minContainer * imgRatio);
                        _this.img.height(minContainer);
                    }
                }
            }

            var isIE6or7 = $.browser.msie && ($.browser.version == 6 || $.browser.version == 7);
            if (isIE6or7) {
                var paddingleft = 0;
                var photocontainer = $(".tn-photo-main", _this.photoViewer)
                if (this.horizontalDirection) {
                    paddingleft = (photocontainer.width() - _this.img.width()) / 2;
                } else {
                    paddingleft = (photocontainer.width() - _this.img.height()) / 2;
                }
                $("div.tn-photo-container").css("text-align", "left").css("padding-left", paddingleft + "px");
            }

            _this.img.data("left", _this.img.position().left);
            _this.img.data("top", _this.img.position().top);

            var LowerThanIE9 = $.browser.msie && $.browser.version < 9;
            if (LowerThanIE9) {
                $("span[id^=photoImage-]", _this.photoViewer).replaceWith(_this.img);
                $("span[id^=photoImage-]", _this.photoViewer).remove();
                var $i = _this.img.next();
                if ($i.is(".tn-ie-center")) {
                    $i.remove();
                }
                _this.img.css("display", "inline").css("visibility", "visible");

                if (this.horizontalDirection) {
                    _this.img.data("left", _this.img.position().left);
                    _this.img.data("top", _this.img.position().top);
                } else {
                    _this.img.data("left", _this.img.position().left + (_this.img.width() - _this.img.height()) / 2);
                    _this.img.data("top", _this.img.position().top + (_this.img.height() - _this.img.width()) / 2);
                }

                _this.img.data("showWidth", _this.img.width());
                _this.img.data("showHeight", _this.img.height());

                var angle = _this.img.data("angle") || 0;
                var isIE6 = $.browser.msie && $.browser.version == 6;
                if (!isIE6)
                    _this.img.rotate(angle);

            }

            _this.addRingFrame();
        },

        /*
        * 以预加载的方式显示照片，并且使其自适应显示区域大小
        */
        loadPhoto: function () {
            var _this = this;

            //如果重复操作，则不处理
            if (_this.indexOld && _this.indexOld == _this.index) {
                return false;
            }

            _this.indexOld = _this.index;

            //加载侧边栏中的内容
            $Side = $("#DetailPhotoSide", _this.photoViewer);
            if ($Side.is(":visible")) {
                $.get($Side.attr("link") + "&a=" + new Date().getTime(), { photoId: _this.photos[_this.index].photoId }, function (data) {
                    $Side.empty().append(data);
                    _this.addRingFrame();
                    _this.toggleSideTag();
                    _this.resizeComment();
                });
            }

            //切换缩略图的分页
            if (_this.index < _this.indexFrom) {
                _this.pageIndex--;
                _this.loadThumbnails();
            }
            if (_this.index > _this.indexTo) {
                _this.pageIndex++;
                _this.loadThumbnails();
            }

            //定位缩略图的位置
            $(".tn-thumb-list li", _this.photoViewer).removeClass("tn-selected");
            $(".tn-thumb-list li#thumbnail-" + _this.index, _this.photoViewer).addClass("tn-selected");

            //图片可能已经旋转过，需要回复水平位置
            _this.rotateReset();

            var photo = _this.photos[_this.index];

            //重置“查看大图”的链接
            $("a.tn-icon-enlarge", _this.photoViewer).attr("href", photo.photoUrl);

            _this.img.attr("src", photo.smallPhotoUrl);

            //先加载小图
            _this.img.data("width", 800);
            _this.img.data("height", 600);
            _this.resizePhoto();

            //再加载大图

            var img = $("<img/>");
            img.load(function () {
                var imgWidth = this.width;               //图片实际宽度
                var imgHeight = this.height;             //图片实际高度

                _this.img.attr("src", photo.bigPhotoUrl);

                _this.img.data("width", imgWidth);
                _this.img.data("height", imgHeight);

                _this.resizePhoto();
            });

            img.attr("src", photo.bigPhotoUrl);    //为了解决ie浏览器的bug，需要将这句放在load回调函数之后

            _this.addFootPhotoDetail();
        },

        /*
        * 加载上一张照片
        */
        loadPhotoPrev: function () {
            var _this = this;
            if (_this.index > 0) {
                _this.index--;
                clearTimeout(_this.loadOtherPhoto);
                _this.loadOtherPhoto = setTimeout(function () { _this.loadPhoto(); }, 500);

            } else {
                art.dialog.tips("已经是第一张了", 1.5, 0);
            }
        },

        /*
        * 加载下一张照片
        */
        loadPhotoNext: function () {
            var _this = this;
            if (_this.index < _this.photos.length - 1) {
                _this.index++;
                clearTimeout(_this.loadOtherPhoto);
                _this.loadOtherPhoto = setTimeout(function () { _this.loadPhoto(); }, 500);
            } else {
                //如果正在播放，则停止播放
                _this.stopPlayPhoto();
                art.dialog.tips("已经是最后一张了", 1.5, 0);
            }
        },

        /*
        * 旋转照片
        */
        rotatePhoto: function (degree) {
            var _this = this;
            var angle = _this.img.data("angle") || 0;
            angle = angle + degree;
            var isIE6 = $.browser.msie && $.browser.version == 6;
            if (!isIE6)
                _this.img.rotate(angle);
            _this.img.data("angle", angle % 360);

            _this.horizontalDirection = _this.horizontalDirection ? false : true;

            _this.resizePhoto();
            _this.exitLabelPhoto();
        },

        /*
        * 重置旋转照片为原始水平状态
        */
        rotateReset: function (degree) {
            var _this = this;
            var isIE6 = $.browser.msie && $.browser.version == 6;
            if (!isIE6)
                _this.img.rotate(0);
            _this.img.data("angle", 0);

            _this.horizontalDirection = true;

            _this.resizePhoto();

        },

        /*
        * 侧边栏显示和隐藏
        */
        toggleSide: function () {
            var _this = this;

            $side = $(".tn-photo-side", _this.photoViewer);

            $side.toggle(200, function () {
                _this.resizePhoto();
                if ($side.is(":visible")) {
                    _this.stopPlayPhoto();
                    _this.resizeComment();
                }
            });

            var icon = $(".tn-side-switch .tn-icon", _this.photoViewer);
            icon.toggleClass("tn-smallicon-triangle-right").toggleClass("tn-smallicon-triangle-left");

            if ($side.is(":visible")) {
                if (this.LastPhotoSideId && _this.LastPhotoSideId == _this.photos[_this.index].photoId) {
                    return false;
                }
                $photoSide = $("#DetailPhotoSide", _this.photoViewer);
                $.get($photoSide.attr("link"), { photoId: _this.photos[_this.index].photoId }, function (data) {
                    _this.LastPhotoSideId = _this.photos[_this.index].photoId;
                    $photoSide.empty().append(data);
                    _this.addRingFrame();
                    _this.addPhotoTags();
                    _this.resizeComment();
                });
            }
        },

        /*
        * 加载缩略图列表
        */
        loadThumbnails: function () {
            var _this = this;

            //如果重复操作，则不处理
            if (_this.pageIndexOld && _this.pageIndexOld == _this.pageIndex && _this.pageSizeOld && _this.pageSizeOld == _this.pageSize) {
                return false;
            }

            _this.pageIndexOld = _this.pageIndex;
            _this.pageSizeOld = _this.pageSize;

            _this.indexFrom = (_this.pageIndex - 1) * _this.pageSize;
            _this.indexTo = _this.pageIndex * _this.pageSize - 1;
            if (_this.indexTo >= _this.photos.length) {
                _this.indexTo = _this.photos.length - 1;
            }

            //清空原有的缩略图
            var thumbnailList = $(".tn-thumb-list ul", _this.photoViewer);
            thumbnailList.empty();

            //加载新的缩略图
            var thumbnailTemplate = '<li id="thumbnail-{0}" data-index="{1}" {2}><a href="#"><i class="tn-ie-center"></i><img width="75" height="75" src="{3}"><b class="tn-photo-shade"></b></a></li>';
            for (var i = _this.indexFrom; i <= _this.indexTo; i++) {
                var css = "";
                if (i == _this.index) {
                    css = " class=\"tn-selected\"";
                }

                var thumbnailUrl = _this.photos[i].smallPhotoUrl;
                var thumbnailImage = thumbnailTemplate.format(i, i, css, thumbnailUrl);
                thumbnailList.append(thumbnailImage);
            }
        },

        /*
        * 开始播放缩略图幻灯片
        */
        startPlayPhoto: function () {
            var _this = this;

            if (_this.index < _this.photos.length - 1) {
                var playPhoto = function () {
                    _this.loadPhotoNext();
                };

                $side = $(".tn-photo-side", _this.photoViewer);
                if ($side.is(":visible"))
                    _this.toggleSide();

                _this.intervalId = setInterval(playPhoto, 3000);
                $("#playPhoto", _this.photoViewer).find(".tn-button-text:first").text("停止");
                $("#playPhoto", _this.photoViewer).find(".tn-icon:first").removeClass("tn-icon-play").addClass("tn-icon-pause");
            } else {
                art.dialog.tips('已经是最后一张了', 1.5, 0);
            }
        },

        /*
        * 停止播放缩略图幻灯片
        */
        stopPlayPhoto: function () {
            var _this = this;

            if (_this.intervalId) {
                clearInterval(_this.intervalId);
            }

            $("#playPhoto", _this.photoViewer).find(".tn-button-text:first").text("播放");
            $("#playPhoto", _this.photoViewer).find(".tn-icon:first").removeClass("tn-icon-pause").addClass("tn-icon-play");
        },

        /*
        * 圈人
        */
        labelPhoto: function () {
            _this = this;
            _this.rotateReset();
            _this.stopPlayPhoto();

            _this.removeRingFrame();

            var LowerThanIE9 = $.browser.msie && $.browser.version < 9;

            if (LowerThanIE9) {
                $("span[id^=photoImage-]", _this.photoViewer).replaceWith(_this.img);
            }

            _this.img.data("Jcrop", null);
            if (_this.img.next().is(".jcrop-holder")) {
                _this.img.next().remove();
            }
            _this.img.Jcrop({
                onChange: function () {
                    $("#labelphoto-select").hide().appendTo("div[id^='photo-viewer-']");
                },
                onSelect: _this.showUserSelect,
                setSelect: [20, 20, 50, 50],
                minSize: [50, 50],
                maxSize: [350, 350]
            });
            _this.img.after("<i class='tn-ie-center'></i>");
        },

        //取消圈人状态
        exitLabelPhoto: function () {
            var _this = this;

            var $i = _this.img.next();
            if ($i.is(".tn-ie-center")) {
                $i.remove();
            }
            if (_this.img.next().is(".jcrop-holder")) {
                _this.img.next().remove();
            }
            _this.img.css("display", "inline").css("visibility", "visible");

            var LowerThanIE9 = $.browser.msie && $.browser.version < 9;
            var isIE6 = $.browser.msie && $.browser.version == 6;
            if (LowerThanIE9 && !isIE6) {
                var angle = _this.img.data("angle") || 0;
                _this.img.rotate(angle);
            }

            var select = $("#labelphoto-select");
            if (select == null)
                return;
            select.hide().appendTo("div[id^='photo-viewer-']");
        },

        //添加圈人边框
        addRingFrame: function () {
            var _this = this;
            _this.removeRingFrame();

            clearTimeout(_this.animateTimeOut);

            $("#photo-message").html((_this.img.data("angle") || 0) % 360);

            var $PhotoId = _this.photos[_this.index].photoId;

            if (_this.img.data($PhotoId) != null) {
                var $imge = _this.img;
                var data = _this.img.data($PhotoId);

                $("#PhotoLabelSide-label").empty();

                var $photoContainer = $("div.tn-photo-container", _this.photoViewer);

                for (var i = 0; i < data.length; i++) {
                    var w = data[i].AreaWidth * _this.scaling;
                    var h = data[i].AreaHeight * _this.scaling;
                    var x = data[i].AreaX * _this.scaling;
                    var y = data[i].AreaY * _this.scaling;

                    var itemH = h;
                    var itemW = w;
                    var itemX = x;
                    var itemY = y;

                    var $itemAngle = (_this.img.data("angle") || 0) % 360
                    if ($itemAngle == -90 || $itemAngle == 270) {
                        itemH = w;
                        itemW = h;
                        itemX = y;
                        itemY = (_this.img.data("showWidth") || _this.img.width()) - x - data[i].AreaWidth * _this.scaling;
                    } else if ($itemAngle == 180 || $itemAngle == -180) {
                        itemH = h;
                        itemW = w;
                        itemX = (_this.img.data("showWidth") || _this.img.width()) - x - (data[i].AreaWidth * _this.scaling);
                        itemY = (_this.img.data("showHeight") || _this.img.height()) - y - (data[i].AreaHeight * _this.scaling);
                    } else if ($itemAngle == 90 || $itemAngle == -270) {
                        itemH = w;
                        itemW = h;
                        itemX = (_this.img.data("showHeight") || _this.img.height()) - y - (data[i].AreaHeight * _this.scaling);
                        itemY = x;
                    }

                    w = itemW;
                    h = itemH;
                    x = itemX + $imge.data("left");
                    y = itemY + $imge.data("top");

                    if ($("#photoLabelModel").html())
                        $($("#photoLabelModel").html().format("photoLabel-" + data[i].LabelId, w, h, data[i].ObjectName)).appendTo($photoContainer);

                    if ($.browser.msie) {
                        $("#photoLabel-" + data[i].LabelId + " .tn-photo-note-circle").width(w).height(h);
                    }

                    $("#photoLabel-" + data[i].LabelId).css("left", x + "px");
                    $("#photoLabel-" + data[i].LabelId).css("top", y + "px");
                    $("div.tn-photo-note-name", $("#photoLabel-" + data[i].LabelId)).css("margin-left", function () {
                        return -$(this).width() / 2;
                    });
                }
                _this.addSideRingFrame();
                _this.animateTimeOut = setTimeout(function () {
                    $("div[id^='photoLabel-']").animate({ opacity: "0" }, 10);
                }, 1000);

                $("div[id^='photoLabel-'] div.tn-photo-note-circle").mouseenter(function () {
                    $(this).parents("div[id^='photoLabel-']").animate({ opacity: "100" }, 10);
                });

                $("div[id^='photoLabel-'] div.tn-photo-note-circle").mouseleave(function () {
                    $(this).parents("div[id^='photoLabel-']").animate({ opacity: "0" }, 10);
                });
            } else {

                if (_this.isLoadingLabel) {
                    setTimeout(function () {
                        _this.addRingFrame();
                    }, 500);
                    return;
                }
                _this.isLoadingLabel = true;
                $.get(_this.photolabelslink, { "photoId": $PhotoId }, function (data) {
                    var $imge = _this.img;
                    _this.img.data($PhotoId, data);

                    _this.isLoadingLabel = false;
                });
            }
        },

        //添加侧边圈人窗口
        addSideRingFrame: function () {
            //如果原来的内容正好是五个，并且，数据是大于五的，那么将内容全部展示出来
            var _this = this;
            var $PhotoId = _this.photos[_this.index].photoId;
            var data = _this.img.data($PhotoId);
            var showCount = 5;
            if (data != null) {
                $("#photoLabelCount").html(data.length > 0 ? data.length : "");
                if (data.length > showCount && $("#PhotoLabelSide-label").children().length == showCount + 1) {
                    $("#PhotoLabelSide-label").empty();
                    for (var i = 0; i < data.length; i++) {
                        if ($("#PhotoLabelSideModel").html())
                            $($("#PhotoLabelSideModel").html().format(data[i].LabelId, data[i].UserLink, data[i].ObjectName, data[i].CanEdit ? "<a href='#' name='delPhotoLabel' value='" + data[i].LabelId + "' class='tn-icon tn-smallicon-cross'></a>" : "")).appendTo($("#PhotoLabelSide-label"));
                    }
                    $("<li><a class='tn-icon tn-smallicon-triangle-down' href='#' id='toggle-PhotoLabel'></a></li>").appendTo($("#PhotoLabelSide-label"));
                } else {
                    $("#PhotoLabelSide-label").empty();
                    var $length = data.length > showCount ? showCount : data.length;
                    for (var i = 0; i < $length; i++) {
                        if ($("#PhotoLabelSideModel").html())
                            $($("#PhotoLabelSideModel").html().format(data[i].LabelId, data[i].UserLink, data[i].ObjectName, data[i].CanEdit ? "<a href='#' name='delPhotoLabel' value='" + data[i].LabelId + "' class='tn-icon tn-smallicon-cross'></a>" : "")).appendTo($("#PhotoLabelSide-label"));
                    }
                    if (data.length > showCount) {
                        $("<li><a class='tn-icon tn-smallicon-triangle-up' href='#' id='toggle-PhotoLabel'></a></li>").appendTo($("#PhotoLabelSide-label"));
                    }
                }
            }
        },

        //移除掉选人信息
        removeRingFrame: function () {
            $("div[id^='photoLabel-']").remove();
        },

        //显示圈人选人框
        showUserSelect: function (item) {
            if (!item)
                return;

            var _this = window._this;
            var select = $("#labelphoto-select");
            if (!select)
                return;

            var $imge = $("div.jcrop-holder");
            if ($imge == null || !$imge.length)
                return;

            select.appendTo("body");
            select.show();
            select.css("left", ($imge.offset().left + item.x2) + "px");
            select.css("top", ($imge.offset().top + item.y) + "px");
            $("#height", select).val(item.h / _this.scaling);
            $("#width", select).val(item.w / _this.scaling);
            $("#left", select).val(item.x / _this.scaling);
            $("#top", select).val(item.y / _this.scaling);
            $("#photoId", select).val(_this.photos[_this.index].photoId);

            $("span#photo-message").html(_this.scaling);
        },

        //删除照片圈人
        deletePhotoLabel: function (item) {
            var _this = this;
            var url = $("#delePhotoLabelLink").attr("href");
            art.dialog.confirm('您确认要删除吗？', function () {
                $.post(url, { labelId: item.attr("value") }, function (data) {
                    if (data.MessageType == 1) {
                        _this.img.removeData(_this.photos[_this.index].photoId);
                        _this.addRingFrame();
                        _this.addRingFrame();
                    } else {
                        art.dialog.tips(data.MessageContent, 1.5, data.MessageType);
                    }
                });
            });
        },

        //添加照片的标签
        addPhotoTags: function () {
            var _this = this;
            var $sideTag = $("#PhotoLabelSide-Tag", _this.photoViewer);
            var url = $sideTag.attr("link");
            $("a[name='showMorePhotoTag']", $sideTag).remove();

            //调整显示对象
            $("div[name='PhotoTagEdit']", $sideTag).hide();
            $("a[name='PhotoTagEdit']", $sideTag).show();

            
            $.get(url + "&a=" + new Date().getTime(), function (data) {
                $sideTag.data("tag", data);
                _this.toggleSideTag();
            });
        },

        //右侧标签的收放
        toggleSideTag: function () {
            var _this = this;
            var $sideTag = $("#PhotoLabelSide-Tag", _this.photoViewer);

            var $data = $sideTag.data("tag");
            if ($data) {
                var $div = $("<div/>")

                $("#addTags", $sideTag).remove();
                var $showCount = 5;
                //原来是收缩起来的情况
                if ($data.length > $showCount && $sideTag.children("a.tn-tag").length == $showCount) {
                    //清除原数据
                    $("a.tn-tag", $sideTag).remove();
                    $("a[name='showMorePhotoTag']", $sideTag).remove();
                    for (var i = 0; i < $data.length; i++) {
                        $("<a href='{0}' target='_blank' class='tn-tag'>{1}</a>".format($data[i].link, $data[i].TagName)).appendTo($div);
                    }
                    $("<a name='showMorePhotoTag' class='tn-icon tn-smallicon-triangle-up' title='收起更多' href='#'>收起更多</a>").appendTo($div);
                } else {
                    //清除原数据
                    $("a.tn-tag", $sideTag).remove();
                    $("a[name='showMorePhotoTag']", $sideTag).remove();
                    var length = $data.length > $showCount ? $showCount : $data.length;
                    for (var i = 0; i < length; i++) {
                        $("<a href='{0}' target='_blank' class='tn-tag'>{1}</a>".format($data[i].link, $data[i].TagName)).appendTo($div);
                    }
                    if ($data.length > $showCount) {
                        $("<a name='showMorePhotoTag' class='tn-icon tn-smallicon-triangle-down' title='展开更多' href='#'>展开更多</a>").appendTo($div);
                    }

                    if ($data.length == 0) {
                        $("<span id='addTags' style='float: left; margin: 8px 0px;'>添加标签</span>").appendTo($div);
                    }
                }

                $($div.html()).prependTo($sideTag);
            }
        },

        //照片操作的显示与隐藏
        togglePhotoOperating: function () {
            var _this = this;
            var $photoOperating = $("#PhotoOperating");
            if ($photoOperating.is(":empty")) {
                $photoOperating.html("<div class='tn-loading'></div>");
                if (!$("#FootPhotoDetailModel-Actions").is(":empty")) {
                    $.get($photoOperating.attr("link"), { photoId: _this.photos[_this.index].photoId }, function (data) {
                        if (data != null) {
                            $photoOperating.empty().append($("#FootPhotoDetailModel-Actions").html().format(data.photoId, !data.isCover, data.isCover ? "取消封面" : "设为封面", !data.isEssential, data.isEssential ? "取消加精" : "加精"));
                        }
                    });
                }
            }
            if ($photoOperating) {
                $photoOperating.show();
                $(document).bind("click", function (e) {
                    if ($(e.target).closest($photoOperating).length > 0 || $(e.target).closest($("#EditPhoto")).length > 0) {
                        return;
                    }
                    $(document).unbind("click", arguments.callee);
                    $photoOperating.hide();
                });
            }
            return false;
        },

        //添加右下角的照片信息
        addFootPhotoDetail: function () {
            var _this = this;
            var data = _this.photos[_this.index];
            var $footPhotoDetail = $("#footPhotoDetail", _this.photoViewer);

            //清除数据
            var $dl = $("#footPhotoDetail-from", $footPhotoDetail).next();
            if ($dl && $dl.is("dl"))
                $dl.remove();

            var $photoOperating = $("#PhotoOperating", $footPhotoDetail);
            if ($photoOperating)
                $photoOperating.empty();

            var $photoDateCreat = $("#FootPhotoDetailModel-DateCreat");
            $("#footPhotoDetail-from", $footPhotoDetail).after($photoDateCreat.html().format(data.dateCreated));
        }
    }
})(jQuery);
