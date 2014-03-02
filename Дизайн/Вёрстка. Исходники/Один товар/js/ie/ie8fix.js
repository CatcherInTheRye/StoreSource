$(function(){
    $('.enter_icon').click(function(){
        $('.login').show();
        $('.login').css({'left':$('.enter_icon').offset().left-275, 'top':$('.enter_icon').offset().top});
    });
    $('.exit_icon').click(function(){
        $('.exit').show();
        $('.exit').css({'left':$('.exit_icon').offset().left-178, 'top':$('.exit_icon').offset().top-20});
    });

    $('.login .close').click(function(){$('.login').hide();});
    $('.exit .close').click(function(){$('.exit').hide();});
})
