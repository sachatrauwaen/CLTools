$(document).ready(function() {

    //return;   
    if ($('.SkinObject #DnnPages').length) {

        var mi = '<li class="root" id="dnnLocalization">';
        mi = mi + '<a href="#">Localization</a>';
        mi = mi + '<div class="megaborder">';

        //mi = mi + '<div id="dnn_cp_RibbonBar_LocalizationPanel" class="cpcbLocalization dnnClear">';
        //mi = mi + '</div>';

        mi = mi + '</div>';
        mi = mi + '</li>';

        $(".dnnadminmega").append(mi);

        $('.SkinObject #DnnPages').detach().appendTo('#dnnLocalization .megaborder');

        $("#dnnLocalization a").hover(function() {


            /*   
            $(this).parent().find('.megaborder').dialog({
            title : "Content localizaton tools",
            modal: true,
            autoOpen: true,
            dialogClass: "dnnFormPopup",
            position: "top",
            minWidth: 910,
            minHeight: 300,
            maxWidth: 1920,
            maxHeight: 1080,
            resizable: true,
            closeOnEscape: true,
            zIndex: 100000,
            //refresh: refresh,
            //closingUrl: closingUrl,
            //close: function (event, ui) { dnnModal.closePopUp(refresh, closingUrl); }
            });
            */


            $(this).parent().find('.megaborder').stop().fadeTo('fast', 1).show().ready(function() {
                CalculateWidth();
            });

        }, function() {

        }
   );

        $("#dnnLocalization .megaborder").hoverIntent(function() {

        }, function() {
            //$(this).stop().fadeTo('fast', 1).hide(); //Find sub and fade it in
        }
    );

        $("#CloseLocalization").click(function() {
            $("#dnnLocalization .megaborder").stop().fadeTo('fast', 1).hide(); //Find sub and fade it in
        });

    }

    /*   
    $(".shared").hoverIntent(
    function() {
        
    var elems = $(this).parent().parent().parent().parent().parent().children().children();
    var maxheight = Math.max.apply(Math, $(elems).map(function(){ return $(this).width(); }).get());     
            
    if ($("input:checked",this).length > 0)
    $(elems).height(maxheight);
        
    }, function() {
    }
    
    );
    
    $(".shared input:not(:checked)").parent().click(
    function() {
        
    var elems = $(this).parent().parent().parent().parent().parent().children().children();
    var maxheight = Math.max.apply(Math, $(elems).map(function(){ return $(this).width(); }).get());     
    $(elems).height(maxheight);

        }    
    );





    $(".shared").parent().parent().parent().parent().parent().hover(
    function() {            
        
    }, function() {
    $(this).children().children().height("22px");
    $(".shared input", this).each(function(index) {
    $(this).attr('checked', ($(this).parent().parent().parent().parent().find(".TreeView input:checked").length > 1) );                        
    });
    }
    );
    */

    $(".shared input").parent().click(
        function() {

            var elems = $(this).parent().parent().parent().parent().parent().children().children();

            if ($(elems).height() == 22) {
                
                //var maxheight = Math.max.apply(Math, $(elems).map(function() { return $(this).width(); }).get());
                //$(elems).height(maxheight);

                $(elems).height("220px");

                $(".shared input").each(function(index) {
                    $(this).attr('checked', ($(this).parent().parent().parent().parent().find(".TreeView input:checked").length > 1));
                });

            } else {

                $(elems).height("22px");
                
                $(".shared input").each(function(index) {
                    $(this).attr('checked', ($(this).parent().parent().parent().parent().find(".TreeView input:checked").length > 1));
                });
            }


        }
    );




    $(".PageInfo input").hoverIntent(
        function() {
            $(".PageInfo").height("138px");
        },
        function() {

        }
    );

    $(".PageInfo").parent().parent().hoverIntent(
        function() {

        },
        function() {
            $(".PageInfo").height("22px");
        }
    );

    $(".AllPagesTranslated").click(
        function() {
            $(".PageTranslated input").attr('checked', "true");
        }
    );

    $(".AllModulesTranslated").click(
        function() {
            $(".ModuleTranslated input").attr('checked', "true");
        }
    );

    $(".cbDetached").click(
        function() {
            if ($(this).is(":checked")) {
                $(".colDetached").show();
                CalculateWidth();
                setCookie("cbDetached", "y", 7);
            }
            else {
                $(".colDetached").hide();
                CalculateWidth();
                setCookie("cbDetached", "n", 7);
            }
        }
    );
    $(".cbDetached").attr('checked', getCookie("cbDetached") == "y");

    if (getCookie("cbDetached") == "y")
        $(".colDetached").show();
    else
        $(".colDetached").hide();


    $(".cbTranslated").click(
        function() {
            if ($(this).is(":checked")) {
                $(".colTranslated").show();
                CalculateWidth();
                setCookie("cbTranslated", "y", 7);
            }
            else {
                $(".colTranslated").hide();
                CalculateWidth();
                setCookie("cbTranslated", "n", 7);
            }
        }
    );
    $(".cbTranslated").attr('checked', getCookie("cbTranslated") == "y");

    if (getCookie("cbTranslated") == "y")
        $(".colTranslated").show();
    else
        $(".colTranslated").hide();


    $(".cbShared").click(
        function() {
            if ($(this).is(":checked")) {
                $(".colShared").show();
                CalculateWidth();
                setCookie("cbShared", "y", 7);
            }
            else {
                $(".colShared").hide();
                CalculateWidth();
                setCookie("cbShared", "n", 7);
            }
        }
    );
    $(".cbShared").attr('checked', getCookie("cbShared") == "y");

    if (getCookie("cbShared") == "y")
        $(".colShared").show();
    else
        $(".colShared").hide();

    $(".AutoTranslate").click(
        function() {

            $("#form-auto-translate").toggle();

        }
    );




    CalculateWidth();
});

function CalculateWidth(){
    //$(".table").width( ($(".tr:first .td").length * ($(".tr:first .td").width()+11)));
    
    
    /*
    $(".tr:first .td").each(function(i){
        w = w + $(this).width()+11;
    });
    */
    
    
    
    $(".table").each(function(i) {
        var w = 0;
        var c = 0;
        if( $(".cbShared").is(":checked") )
            c = c + 20
            
        w = w + 1 + 5 + 155 + 5 + 5 + c;
        
        if( $(".cbDetached").is(":checked") )
            c = c + 20
        
        if( $(".cbTranslated").is(":checked") )
            c = c + 20
        
        w = w + ( ($(".tr:first .td", this).length-1) * (1+ 5+ 155 + 5 + 5 + c) );
        
        $(this).width(w);
    });
    
    
    
    var h = 0;
    $(".td").each(function(i){
        $(".PageDescription", this).width(148);
        $(".PageDescription", this).width($(".PageTitle", this).width());
       
       $(".AddToPages", this).width(148);
       $(".AddToPages", this).width($(this).width()); 
       
       var thisHeight = $(".AddToPages", this).height();
       if( thisHeight > h) {
			h = thisHeight;
		}
        
    });
    //$(".AddToPages", this).height(h); 
    
}

function setCookie(name,value,days) {    
if (days) {        
var date = new Date();        
date.setTime(date.getTime()+(days*24*60*60*1000));        
var expires = "; expires="+date.toGMTString();    
}    
else 
var expires = "";    
document.cookie = name+"="+value+expires+"; path=/";
}

function getCookie(name) {    
var nameEQ = name + "=";    
var ca = document.cookie.split(';');    
for(var i=0;i < ca.length;i++) {        
var c = ca[i];        
while (c.charAt(0)==' ') c = c.substring(1,c.length);        
if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length,c.length);    
}    
return null;
}