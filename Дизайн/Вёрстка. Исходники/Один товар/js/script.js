var left_slider;
var right_slider;
$(document).ready(function(){
    // script dlya krossbrazernogo placeholder
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

    // opredelyaem razmer bolshoi kartinki (product.html)
    if ($('.big_img img').length>0){
        var w = $('.big_img img').width();
        var h = $('.big_img img').height();
        if (w>h){
            $('.big_img img').css({'width':'70%'});
            if($('.big_img img').height()>$('.big_img').height()){$(this).css({'width':'auto','height':'90%'});}
        }else{
            $('.big_img img').css({'height':'90%'});
            if($('.big_img img').width()>($('.big_img').width()*0.8)){$(this).css({'height':'auto','width':'70%'});}
        }
    }


    // okoshki vhoda vihoda
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

    $('nav>ul>li>ul>li>a').click(function(){$(this).parent().toggleClass('active')});

    $('.tovari ul li').hover(function(){
        $('.zoom').find('img').attr('src',$(this).find('img').attr('src'));
        var w = $(this).find('img').width();
        var h = $(this).find('img').height();
        $('.zoom').find('img').css('width', w );
        $('.zoom').find('img').css('height', h );

        $('.zoom').find('img').attr('src',$(this).find('img').attr('src'));
        $('.zoom').css({'left':$(this).offset().left-20,'top':$(this).offset().top-20,'opacity':'0.8'});
        $('.zoom').show().stop(true).animate({'opacity':'1'},200);
        $('.zoom img').stop(true).animate({'width':w+40, 'height':h+40},200);
    });
    $('.zoom').hover(function(){}, function(){$('.zoom').hide()});

    //verhnee menu
    $('nav>ul>li>a').click(function(){
        $(this).parent().toggleClass('active');
        if($(this).parent().hasClass('active')){
            $('nav>ul>li').removeClass('active');
            $(this).parent().addClass('active');
            $('.drop_ul').children('div').html('');
            if  ($('.drop_ul').css('display')=='none'){
                if($(this).parent().children('ul').length>0){$('.drop_ul').show();}
                var lis = $(this).parent().children('ul').children('li');
                lis.each(function(){
                    if ((window).innerWidth>1090){
						if($(this).index()==0){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
						if($(this).index()==1){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
						if($(this).index()==2){$('.drop_ul').children('.colonka4').append('<div class="block">'+$(this).html()+'</div>')}
						if($(this).index()==3){$('.drop_ul').children('.colonka2').append('<div class="block">'+$(this).html()+'</div>')}
						if($(this).index()==4){$('.drop_ul').children('.colonka3').append('<div class="block">'+$(this).html()+'</div>')}					
					}else{
						if($(this).index()==0){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
						if($(this).index()==1){$('.drop_ul').children('.colonka1').append('<div class="block">'+$(this).html()+'</div>')}
						if($(this).index()==2){$('.drop_ul').children('.colonka2').append('<div class="block">'+$(this).html()+'</div>')}
						if($(this).index()==3){$('.drop_ul').children('.colonka2').append('<div class="block">'+$(this).html()+'</div>')}
						if($(this).index()==4){$('.drop_ul').children('.colonka3').append('<div class="block">'+$(this).html()+'</div>')}
					}
                })
            }else{$('.drop_ul').hide();}
        }else{$('.drop_ul').hide()};
    });

    //scroll vverh
    $('.go_top').click(function(){$('body,html').animate({scrollTop: 0}, 300);})


    //inicializaciya levogo slidera (product.html)
    if (!$('html').hasClass('ie8')){
        if($('.product_var').length>0){
            if ((window).innerWidth>1200){
                left_slider = $('.product_var').bxSlider({minSlides: 4,maxSlides: 4,slideWidth: 130, touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true })
            }
            else{
                if ((window).innerWidth>799){
                    left_slider = $('.product_var').bxSlider({minSlides: 3,maxSlides: 3,slideWidth: 130, touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})
                }
                else{
                    left_slider = $('.product_var').bxSlider({minSlides: 1,maxSlides: 1,slideWidth: 1000, touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})
                }
            }
        }
        //inicializaciya pravogo slidera (product.html)
        if($('.product .right_part .alike_wrap ul').length>0){
            if ((window).innerWidth>1200){
                if ((window).innerWidth>1750){
                    right_slider = $('.product .right_part .alike_wrap ul').bxSlider({ minSlides: 4, maxSlides: 4,slideWidth: 300, touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})
                }else{
                    if ((window).innerWidth>1350){right_slider = $('.product .right_part .alike_wrap ul').bxSlider({ minSlides: 3, maxSlides: 3,slideWidth: 300, touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})}
                    else{right_slider = $('.product .right_part .alike_wrap ul').bxSlider({ minSlides: 2, maxSlides: 2,slideWidth: 180, touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})}
                }
            }else{
                if ((window).innerWidth>799){
                    right_slider = $('.product .right_part .alike_wrap ul').bxSlider({minSlides: 2, maxSlides: 2, slideWidth: 180,touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})
                }else{
                    if ((window).innerWidth>600){
                        right_slider = $('.product .right_part .alike_wrap ul').bxSlider({minSlides: 3,maxSlides: 3,slideWidth: 300, touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true })
                    }else{
                        right_slider = $('.product .right_part .alike_wrap ul').bxSlider({minSlides: 2,maxSlides: 2,slideWidth: 180,touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true })
                    }
                }
            }
        }
    }
    else{
        //inicializaciya sliderov dlya IE (product.html)
        left_slider = $('.product_var').bxSlider({minSlides: 4,maxSlides: 4,slideWidth: 130, touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true}) ;
        right_slider = $('.product .right_part .alike_wrap ul').bxSlider({ minSlides: 3, maxSlides: 3,slideWidth: 180, touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})
    }


    $('.product_var').on('click','li a',function(e){
        var link = $(this).attr('href');
        $('.product_var li').removeClass('active');
        $(this).parent().addClass('active');
        if (((window).innerWidth>799)||$('html').hasClass('ie8')){
            $('.big_img img').animate({'opacity':'0'},'fast',function(){
                var pic = new Image();
                pic.src = link ;
                $(pic).load(function() {
                    $('.big_img').html(pic);
                    var w = $('.big_img img').width();
                    var h = $('.big_img img').height();
                    if (w>h){
                        $('.big_img img').css({'width':'70%'});
                        setTimeout(function(){if($('.big_img img').height()>$('.big_img').height()){$(this).css({'width':'auto','height':'100%'});}},50);
                    }else{
                        $('.big_img img').css({'height':'100%'});
                        setTimeout(function(){if($('.big_img img').width()>($('.big_img').width()*0.8)){$(this).css({'height':'auto','width':'70%'});}},50);
                    }
                    $('.big_img img').animate({'opacity':'1'},'fast')
                });
                ;});

        }else{
            $('.big_big_img').show(0,function(){
                $('.big_big_img img').attr('src',link);
                var pic = new Image();
                pic.src = link ;
                $(pic).load(function() {
                    $('.big_img').html(pic);
                    var img_w = $('.big_big_img img').width();
                    var img_h = $('.big_big_img img').height();
                    if (img_w>img_h){
                        $('.big_big_img img').css({'width':'90%'});
                        if($('.big_big_img img').height()>$('.big_big_img .wrap').height()){$('.big_big_img img').css({'width':'auto','height':'90%'});}
                    }else{
                        $('.big_big_img img').css({'height':'90%'});
                        if($('.big_big_img img').width()>$('.big_big_img .wrap').width()){$('.big_big_img img').css({'height':'auto','width':'90%'});}
                    }
                    $('.big_big_img img').css('margin-top',(($('.big_big_img .wrap').height()-$('.big_big_img img').height())/2))
                })
            });
        }
    });
    $('.product .big_img').on('click','img',function(){
        $('.big_big_img img').removeAttr('style');
        $('.big_big_img').show(0,function(){
            var pic = new Image();
            pic.src = $('.product .big_img img').attr('src') ;
            $(pic).load(function() {
                $('.big_big_img .wrap').html(pic);
                var img_w = $('.big_big_img img').width();
                var img_h = $('.big_big_img img').height();
                if (img_w>img_h){
                    $('.big_big_img img').css({'width':'90%'});
                    if($('.big_big_img img').height()>$('.big_big_img .wrap').height()){$('.big_big_img img').css({'width':'auto','height':'90%'});}
                }else{
                    $('.big_big_img img').css({'height':'90%'});
                    if($('.big_big_img img').width()>$('.big_big_img .wrap').width()){$('.big_big_img img').css({'height':'auto','width':'90%'});}
                }
                $('.big_big_img img').css('margin-top',(($('.big_big_img .wrap').height()-$('.big_big_img img').height())/2))
            })
        });
    });


     $('.big_big_img').click(function(){$('.big_big_img').hide();$('.big_big_img img').removeAttr('style')});


    $('.product .right_part .tabs_titles li a').click(function(){
        var cur = $(this);
        $('.product .right_part .current_tab_title').text(cur.text());
        $('.product .right_part .tabs_content').animate({'opacity':'0'},'fast',function(){
            $('.product .right_part .tabs_titles li,.product .right_part .tabs_content>li').removeClass('active');
            $('.product .right_part .tabs_content>li').eq(cur.parent().index()).addClass('active');
            cur.parent().addClass('active');
            $('.product .right_part .tabs_content').animate({'opacity':'1'},'fast')
        });
        if ((window).innerWidth<799){
            $('.product .right_part .tabs_titles').hide(); $('.cover').hide();
        }
    });
    $('.product .right_part .current_tab_title').click(function(){$('.product .right_part .tabs_titles').show(); $('.cover').show()});
	$('.cover').click(function(){$(this).hide();  $('.product .right_part .tabs_titles').hide();});















    $(window).resize(function(){
        $('.login').css({'left':'0','top':'0'});
        $('.exit').css({'left':'0','top':'0'});
        if ((window).innerWidth>799){$('nav').css('display','block'); if($('.product .right_part .tabs_titles').length>0){$('.product .right_part .tabs_titles').show();$('.cover').hide();}}

        //perezapusk sliderov
        if (!$('html').hasClass('ie8')){
            if($('.product_var').length>0){
                if ((window).innerWidth>799){
                    if ((window).innerWidth>1200){
                        left_slider.reloadSlider({ minSlides: 4,maxSlides: 4, slideWidth: 130,touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true});
                    }else{
                        left_slider.reloadSlider({minSlides: 3,maxSlides: 3,slideWidth: 130,touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true});
                    }
                }
                else{
                        left_slider.reloadSlider({minSlides: 1,maxSlides: 1,slideWidth: 5000,touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true});
                }
                if ((window).innerWidth>1750){
                    right_slider.reloadSlider({minSlides: 4, maxSlides: 4,slideWidth: 300,touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})
                }else{
                    if ((window).innerWidth>1350){right_slider.reloadSlider({minSlides: 3, maxSlides: 3,slideWidth: 300,touchEnabled: true, pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true})}
                    else{
                        if ((window).innerWidth>799){
                            right_slider.reloadSlider({minSlides: 2, maxSlides: 2, slideWidth: 180,touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true});
                        }else{
                            if ((window).innerWidth>600){
                                right_slider.reloadSlider({minSlides: 3, maxSlides: 3, slideWidth: 300,touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true});
                            }else{
                                right_slider.reloadSlider({minSlides: 2, maxSlides: 2, slideWidth: 180,touchEnabled: true,pager:false, moveSlides: 1, infiniteLoop: false,hideControlOnEnd: true});
                            }
                        }
                    }
                }
            }
        }else{
            $('.login').css({'left':$('.enter_icon').offset().left-275, 'top':$('.enter_icon').offset().top});
            $('.exit').css({'left':$('.exit_icon').offset().left-178, 'top':$('.exit_icon').offset().top-20});
        }





        if (((window).innerWidth>799)&&((window).innerWidth<1391)){
            $('.login').css({'left':$('.enter_icon').offset().left-275, 'top':$('.enter_icon').offset().top});
            $('.exit').css({'left':$('.exit_icon').offset().left-178, 'top':$('.exit_icon').offset().top-20});
        }
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

        if($('.big_big_img').length>0){$('.big_big_img').hide()}

    });


    $(window).scroll(function(event){
        if ($(window).scrollTop()>100){
            $('.go_top').show();
        }else{
            $('.go_top').hide();
        }
    });

});


