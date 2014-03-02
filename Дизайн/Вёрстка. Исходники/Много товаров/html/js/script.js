$(document).ready(function(){
    $('[placeholder]').focus(function() {
        var input = $(this);
        if (input.val() == input.attr('placeholder')) {
            input.val('');
            input.removeClass('placeholder');
        }
    }).blur(function() {
            var input = $(this);
            if (input.val() == '' || input.val() == input.attr('placeholder')) {
                input.addClass('placeholder');
                input.val(input.attr('placeholder'));
            }
        }).blur().parents('form').submit(function() {
            $(this).find('[placeholder]').each(function() {
                var input = $(this);
                if (input.val() == input.attr('placeholder')) {
                    input.val('');
                }
            })
        });

    $('.enter_icon').click(function(){
        if ((window).innerWidth>799){
        $('.login').addClass('clicked');
        $('.login').css({'left':$('.enter_icon').offset().left-275, 'top':$('.enter_icon').offset().top});}
    });
    $('.exit_icon').click(function(){
        if ((window).innerWidth>799){
            $('.exit').addClass('clicked');
            $('.exit').css({'left':$('.exit_icon').offset().left-178, 'top':$('.exit_icon').offset().top-20});}
    });



    $('.enter_icon').click(function(){$('.login').addClass('clicked');});
    $('.exit_icon').click(function(){$('.exit').addClass('clicked');});

    $('.login .close').click(function(){$('.login').removeClass('clicked');});
    $('.exit .close').click(function(){$('.exit').removeClass('clicked');});


    $('.menu_mobile .menu_button').click(function(){$('nav').slideToggle()});

    $('nav>ul>li>ul>li>a').click(function(){
        $(this).parent().toggleClass('active')
    });
    $('.tovari ul li').hover(function(){
        $('.zoom').find('img').attr('src',$(this).find('img').attr('src'));
        var w = $(this).find('img').width();
        $('.zoom').find('img').css('width', w );

        $('.zoom').find('img').attr('src',$(this).find('img').attr('src'));
        $('.zoom').css({'left':$(this).offset().left-20,'top':$(this).offset().top-20,'opacity':'0.5'});
        $('.zoom').show().stop(true).animate({'opacity':'1'},200);
        $('.zoom img').stop(true).animate({'width':w+40},200);
    });
    $('.zoom').hover(function(){}, function(){$('.zoom').hide()});

    $('nav>ul>li>a').click(function(){
        $(this).parent().toggleClass('active');
        if($(this).parent().hasClass('active')){
            $('.drop_ul').children('div').html('');
            if  ($('.drop_ul').css('display')=='none'){
                if($(this).parent().children('ul').length>0){$('.drop_ul').show();}
                var lis = $(this).parent().children('ul').children('li');
                lis.each(function(){
                    if($(this).index()==0){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==1){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==2){$('.drop_ul').children('.colonka2').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==3){$('.drop_ul').children('.colonka4').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==4){$('.drop_ul').children('.colonka3').append('<div class="block">'+$(this).html()+'</div>')}
                })
            }else{$('.drop_ul').hide();}
        }else{$('.drop_ul').hide()};
    });


    $(window).resize(function(){
        $('.login').removeClass('clicked').css({'left':'0','top':'0'})
        $('.exit').removeClass('clicked').css({'left':'0','top':'0'})
        if ((window).innerWidth>799){$('nav').css('display','block');}

        if (((window).innerWidth>799)&&((window).innerWidth<1090)){
            if  ($('.drop_ul').css('display')=='block'){
                $('.drop_ul').children('div').html('');
                var liz = $('nav>ul>li.active').children('ul').children('li');
                liz.each(function(){
                    if($(this).index()==0){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==1){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==2){$('.drop_ul').children('.colonka2').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==3){$('.drop_ul').children('.colonka2').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==4){$('.drop_ul').children('.colonka3').append('<div class="block">'+$(this).html()+'</div>')}
                })
            }
        }
        if ((window).innerWidth>1090){
            if  ($('.drop_ul').css('display')=='block'){
                $('.drop_ul').children('div').html('');
                var liz = $('nav>ul>li.active').children('ul').children('li');
                liz.each(function(){
                    if($(this).index()==0){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==1){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==2){$('.drop_ul').children('.colonka2').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==3){$('.drop_ul').children('.colonka4').append('<div class="block">'+$(this).html()+'</div>')}
                    if($(this).index()==4){$('.drop_ul').children('.colonka3').append('<div class="block">'+$(this).html()+'</div>')}
                })
            }
        }

    });

});
