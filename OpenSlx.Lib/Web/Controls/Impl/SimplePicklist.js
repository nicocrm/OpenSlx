function OpenSlx_MultiSelectPicklist(txtId, divId, lstId) {    
    $('#' + txtId).focus(function() {
        var txt = $(this);
        var panel = document.getElementById(divId);
        var values = '|' + this.value.replace(/; /g, '|') + '|';
        $(panel).show();
        var pkl = document.getElementById(lstId);
        for (var i = 0; i < pkl.options.length; i++) {
            pkl.options[i].selected = new RegExp('\\|' + pkl.options[i].value + '\\|').test(values);
        }
        pkl.focus();
    });
    var onsave = function() {
        var txt = document.getElementById(txtId);
        var pkl = document.getElementById(lstId);
        var s = [];
        for (var i = 0; i < pkl.options.length; i++) {
            if (pkl.options[i].text && pkl.options[i].selected)
                s.push(pkl.options[i].value);
        }
        var newText = s.join('; ');
        if (newText != txt.value) {
            txt.value = newText;
            $(txt).change();
        }
        $('#' + divId).hide();
    };
    $('#' + lstId).blur(onsave);

    var div = document.getElementById(divId);
    div.className = "slxcontrol";
    $(div).append("<div style='text-align: right; width: 100%;margin: 2px 0 2px 0'><button class='slxbutton'>OK</button></div>");
}