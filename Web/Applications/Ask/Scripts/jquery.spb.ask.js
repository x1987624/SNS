$(document).ready(function () {
    //关注/取消关注
    $("a[id^='subscribe-']").live('click', function (e) {
        e.preventDefault();
        var self = $(this);
        var objectId = self.attr("objectId");
        $.post(self.attr("href"), function (data) {
            if (data.MessageType == '1') {
                $("a[id^='subscribe-'][id$='" + objectId + "']").show();
                $("a[id='"+self.attr('id')+"']").hide();
            } else {
                art.dialog.tips(data.MessageContent, 2, data.MessageType);
            }
        });

        return false;
    });
});