// jQuery and JS Document

var dataTypeSetting = "sync";


$(document).on("pagebeforeshow", function(){       

   	/*$("#defaultpanel").panel("open");*/
	columnAutoResize();
	/*$('#column-right').css('left', $('#column-left').outerWidth());*/
		
});

$(function(){
  //$("#column-left a").attr("rel","external");
    $("a").attr("rel","external");
});

function scrollBarOffset() {	
	/* Check for vertical scroll */	
	var $hOffset = 0;	
	if ($('#column-right').height() >= $(window).height()) {
        $hOffset = 15;		
    } 		
	return $hOffset;
}

function columnAutoResize() {
	
	/*if ($('.intelli-main-content').height() < ($(window).height() - $('.intelli-main-content-header').height() - 60 - 1)) $('.intelli-main-content').css('height', $(window).height() - $('.intelli-main-content-header').height() - 60 - 1);		*/
	/*if (!scrollBarOffset()) $('.intelli-main-content').css('height', $(window).height() - $('.intelli-main-content-header').height() - 60 - 1); */	
	
	var offset = $('#column-left').offset();
    if (offset != null) {
        if (offset.left == 0) {
            $('#column-right').width($(window).width() - $('#column-left').width() - 15 - scrollBarOffset()); 
        }
	    else {
		    $('#column-right').width($(window).width() - 15); 
		    /*$('#column-right').css('left', 0);*/
	    }	
    }
}

$(window).load(function() {

	columnAutoResize();

});

$(document).ready(function(){			    							    

	$('#togglepanel').click(function() {	
		var $lefty = $('#column-left');
		$lefty.animate({
		  left: parseInt($lefty.css('left'),10) == 0 ?	-$lefty.outerWidth() :	0
		});
		$('#column-right').animate({			
			left: parseInt($('#column-right').css('left'),10) == 0 ? $lefty.outerWidth() :	0,			
			width: parseInt($('#column-right').css('left'),10) == 0 ? $(window).width() - $('#column-left').width() - 15 - scrollBarOffset() : $(window).width() - scrollBarOffset(),
		}, 'fast');
	});
	
	/*$('#editor').wysiwyg();
	$('#editor').cleanHtml();*/
	
//	tinymce.init({
//		selector: ".richtext",
//		//mode : "textareas",
//		editor_deselector : "simple",
//		plugins: [
//			"advlist autolink lists link image charmap print preview anchor",
//			"searchreplace visualblocks code fullscreen",
//			"insertdatetime media table contextmenu paste "
//		],
		toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
//	});
	
	$( ".intelli-tabs" ).tabs();
	
	/* Pop Ups*/
	   
	$('.btclose').click(
		function() {
			$(this).parent().css('display', 'none');
			$("div.lb_overlay").css('display', 'none');
			return false;
	});	
//	$('.intelli-btn-popupsave').click(
//		function() {
//			$(this).parent().parent().css('display', 'none');
//			$("div.lb_overlay").css('display', 'none');
//			return false;
//	});
	
	/*$('.intelli-multiselect').selectmenu({ nativeMenu: "true" });*/
		
	//$(".intelli-datapicker").datepicker();
		
});

$(window).resize(columnAutoResize);

$(window).on("orientationchange",function(event){
	
	columnAutoResize();
	
});