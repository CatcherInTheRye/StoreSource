$(function(){
    $('.enter_icon').click(function(){
        $('.login').toggleClass('hovered');
        $('.login').css({'left':$('.enter_icon').offset().left-275, 'top':$('.enter_icon').offset().top});
    });
})