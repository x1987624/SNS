(function ($) {
    //�趨���ͣ����λ��
    $.fn.setCursorPosition = function (position) {
        if (this.lengh == 0) return this;
        return $(this).setSelection(position, position);
    }

    $.fn.setSelection = function (selectionStart, selectionEnd) {
        if (this.lengh == 0) return this;
        input = this[0];

        if (input.createTextRange) {
            var range = input.createTextRange();
            range.collapse(true);
            range.moveEnd('character', selectionEnd);
            range.moveStart('character', selectionStart);
            range.select();
        } else if (input.setSelectionRange) {
            input.focus();
            input.setSelectionRange(selectionStart, selectionEnd);
        }

        return this;
    }

    //����궨λ��¼�������
    $.fn.focusEnd = function () {
        this.setCursorPosition(this.val().length);
    }

    //��ȡһ���ı��������ݣ�showCountTag��ʾ����״̬�Ķ��󣬽�ȡ���������Ƿ��Զ�����
    $.fn.GetTextCount = function (showCountTag, countNum, isCut, isAutoHeight) {
        if (!this) {
            return;
        }
        if (isAutoHeight) {
            this.spbAutoHeight();
        }

        this.focus(function () {
            setTimeOutGetTextCount(this, showCountTag, countNum, isCut, true);
        });
    }
    //ѭ����������
    function setTimeOutGetTextCount(element, showCountId, countNum, isCut, isContinue) {
        var $textCount = $(element).val().length;
        var countElement = document.getElementById(showCountId)
        if (!countElement)
            return;
        countElement.innerHTML = $textCount + "/" + countNum;
        if (isCut) {
            if ($textCount > countNum) {
                element.value = element.value.slice(0, $(element).val().length + countNum - $textCount);
                $(element).focusEnd();
            }
        }
        if (isContinue) {
            setTimeout(function () { setTimeOutGetTextCount(element, showCountId, countNum, isCut, document.activeElement == element) }, 30);
        }
    }

    //����Ӧ�߶�
    $.fn.spbAutoHeight = function (maxHeight) {
        if (!this) {
            return;
        }
        $this = this[0];

        if (!$this) {
            return;
        }

        $this.style.overflow = 'hidden';
        //        this.data("height", $this.scrollHeight);
        this.focus(function () {
            if (!$(this).data("height")) {
                $(this).data("height", this.scrollHeight)
            }
            if (!maxHeight) {
                maxHeight = 600;
            }
            miniHeight = $(this).data("height");
            setTimeoutAutoHeight(this, maxHeight, miniHeight);
        });
    }

    //����¼���ĸ߶�
    function SetTextAreaHeight(element, maxHeight, miniHeight) {
        $(element).parent().css("height", $(element).css("height"));
        element.style.height = $(element).data("height") + 'px';
        $textHeigt = element.scrollHeight;

        if (miniHeight && $textHeigt < miniHeight) {
            $textHeigt = miniHeight;
        }
        if (maxHeight && $textHeigt > maxHeight) {
            $textHeigt = maxHeight;
        }
        element.style.height = $textHeigt + 'px';
        if ($textHeigt >= maxHeight) {
            element.style.overflow = "auto";
        } else {
            element.style.overflow = 'hidden';
        }
        $(element).parent().css("height", $(element).css("height"));
    }

    //ѭ������Ӧ�߶�
    function setTimeoutAutoHeight(element, maxHeight, miniHeight, lastTextCount) {
        var $textCount = $(element).val().length;
        $textCount = $textCount % 1 != 0 ? $textCount + 0.5 : $textCount;
        if (lastTextCount != $textCount) {
            SetTextAreaHeight(element, maxHeight, miniHeight);
        }
        if (document.activeElement == element) {
            setTimeout(function () { setTimeoutAutoHeight(element, maxHeight, miniHeight, $textCount) }, 30);
        } else {
            SetTextAreaHeight(element, maxHeight, miniHeight);
        }
    }
})(jQuery);