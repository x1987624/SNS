/*
* @author jiangshl
* ͼƬ��ֱˮƽ������ʾ
*/
(function ($) {
    $.fn.ImageAutoResize = function (options) {
        var defaults = {
            "width": null,
            "height": null
        };
        var opts = $.extend({}, defaults, options);
        return $(this).each(function () {
            var $this = $(this);
            var objHeight = $this.height(); //ͼƬ�߶�
            var objWidth = $this.width(); //ͼƬ���
            var parentHeight = opts.height || $this.parent().height(); //ͼƬ�������߶�
            var parentWidth = opts.width || $this.parent().width(); //ͼƬ���������
            var ratio = objHeight / objWidth;
            if (objHeight > parentHeight && objWidth > parentWidth) {
                if (objHeight > objWidth) { //��ֵ���
                    $this.width(parentWidth);
                    $this.height(parentWidth * ratio);
                } else {
                    $this.height(parentHeight);
                    $this.width(parentHeight / ratio);
                }
                objHeight = $this.height(); //���»�ȡ���
                objWidth = $this.width();
                if (objHeight > objWidth) {
                    $this.css("top", (parentHeight - objHeight) / 2);
                    //����top����
                } else {
                    //����left����
                    $this.css("left", (parentWidth - objWidth) / 2);
                }
            }
            else {
                if (objWidth > parentWidth) {
                    $this.css("left", (parentWidth - objWidth) / 2);
                }
                $this.css("top", (parentHeight - objHeight) / 2);
            }
        });
    };
})(jQuery);
