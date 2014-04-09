function getRelativePos(mouseEvent, element) {
    var rect = element.getBoundingClientRect();
    var scrollTop = document.documentElement.scrollTop ?
						document.documentElement.scrollTop : document.body.scrollTop;
    var scrollLeft = document.documentElement.scrollLeft ?
						document.documentElement.scrollLeft : document.body.scrollLeft;
    var elementLeft = rect.left + scrollLeft;
    var elementTop = rect.top + scrollTop;

    if (document.all) { //detects using IE   
        //event not evt because of IE
        x = event.clientX + scrollLeft - elementLeft;
        y = event.clientY + scrollTop - elementTop;
        return { x: x, y: y };
    }
    else {
        x = mouseEvent.pageX - elementLeft;
        y = mouseEvent.pageY - elementTop;
        return { x: x, y: y };
    }
}