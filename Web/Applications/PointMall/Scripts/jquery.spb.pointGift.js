$(document).ready(function () {
    //收藏/取消收藏
    $("a[id^='favorite-']").live('click', function (e) {
        e.preventDefault();
        var self = $(this);
        var objectId = self.attr("objectId");
        $.post(self.attr("href"), function (data) {
            if (data.MessageType == '1') {
                $("a[id^='favorite-'][id$='" + objectId + "']").show();
                $("a[id='"+self.attr('id')+"']").hide();
            } else {
                art.dialog.tips(data.MessageContent, 2, data.MessageType);
            }
        });

        return false;
    });



});