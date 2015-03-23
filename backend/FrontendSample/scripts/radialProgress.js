/**
 Copyright (c) 2014 BrightPoint Consulting, Inc.

 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:

 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
 */

function radialProgress(parent) {
    var _data=null,
		_data=null,
        _duration= 1000,
        _selection,
        _margin = {top:0, right:0, bottom:30, left:0},
        __width = 300,
        __height = 300,
        _diameter,
        _label="",
		_label2="",
        _fontSize=5;


    var _mouseClick;

    var _value= 0,
        _minValue = 0,
        _maxValue = 100,
		_value2=0,
		_minValue2 = 0,
		_maxValue2 = 100;

    var  _currentArc= 0, _currentArc2= 0, _currentArc3=0, _currentArc4=0, _currentValue=0, _currentValue2=0;

    var _arc = d3.svg.arc()
        .startAngle(0 * (Math.PI/180)); //just radians

    var _arc2 = d3.svg.arc()
        .startAngle(0 * (Math.PI/180))
        .endAngle(0); //just radians
		
	var _arc3 = d3.svg.arc()
        .startAngle(0 * (Math.PI/180))
        .endAngle(0); //just radians
		
	var _arc4 = d3.svg.arc()
        .startAngle(0 * (Math.PI/180))
        .endAngle(0); //just radians

    _selection=d3.select(parent);


    function component() {

        _selection.each(function (data) {

            // Select the svg element, if it exists.
            var svg = d3.select(this).selectAll("svg").data([data]);

            var enter = svg.enter().append("svg").attr("class","radial-svg").append("g");

            measure();

            svg.attr("width", __width)
                .attr("height", __height);


            var background = enter.append("g").attr("class","component")
                .attr("cursor","pointer")
                .on("click",onMouseClick);


            _arc.endAngle(360 * (Math.PI/180))

            background.append("rect")
                .attr("class","background")
                .attr("width", _width)
                .attr("height", _height);

            background.append("path")
                .attr("transform", "translate(" + _width/2 + "," + _width/2 + ")")
                .attr("d", _arc);
				
			background.append("path")
                .attr("transform", "translate(" + _width/3 + "," + _width/3 + ")")
                .attr("d", _arc4);

            background.append("text")
                .attr("class", "label")
                .attr("transform", "translate(" + _width/2 + "," + (_width + _fontSize/2) + ")")
                .text(_label);
				
			
           var g = svg.select("g")
                .attr("transform", "translate(" + _margin.left + "," + _margin.top + ")");


            _arc.endAngle(_currentArc);
            enter.append("g").attr("class", "arcs");
            var path = svg.select(".arcs").selectAll(".arc").data(data);
            path.enter().append("path")
                .attr("class","arc")
                .attr("transform", "translate(" + _width/2 + "," + _width/2 + ")")
                .attr("d", _arc);

            //Another path in case we exceed 100%
            var path2 = svg.select(".arcs").selectAll(".arc2").data(data);
            path2.enter().append("path")
                .attr("class","arc2")
                .attr("transform", "translate(" + _width/2 + "," + _width/2 + ")")
                .attr("d", _arc2);
				
				            var path3 = svg.select(".arcs").selectAll(".arc3").data(data);
            path3.enter().append("path")
                .attr("class","arc3")
                .attr("transform", "translate(" + _width/2 + "," + _width/2 + ")")
                .attr("d", _arc3);
				
				            var path4 = svg.select(".arcs").selectAll(".arc4").data(data);
            path4.enter().append("path")
                .attr("class","arc4")
                .attr("transform", "translate(" + _width/2 + "," + _width/2 + ")")
                .attr("d", _arc4);


            enter.append("g").attr("class", "labels");
            var label = svg.select(".labels").selectAll(".label").data(data);
            label.enter().append("text")
                .attr("class","label")
                .attr("y",_width/2.5+_fontSize/3)
                .attr("x",_width/2)
                .attr("cursor","pointer")
                .attr("width",_width)
                
                .text(function (d) { return _value+"/"+_minValue /*Math.round((_value-_minValue)/(_maxValue-_minValue)*100) + "%"*/ })
                .style("font-size",_fontSize/3+"px")
                .on("click",onMouseClick);
				
				var imgs = svg.selectAll(".image1").data(data);
            imgs.enter()
            .append("svg:image")
			.attr("y",_width/3.9+_fontSize/3)
            .attr("x",_width/2.2)
			.attr('width', _diameter/15)
   .attr('height', _diameter/15)
   .attr("xlink:href","day.png");
   
   var imgs = svg.selectAll(".image2").data(data);
            imgs.enter()
            .append("svg:image")
			.attr("y",_width/2.3+_fontSize/3)
            .attr("x",_width/2.2)
			.attr('width', _diameter/15)
   .attr('height', _diameter/15)
   .attr("xlink:href","week.png");
				
				var label2 = svg.select(".labels").selectAll(".label2").data(data);
            label2.enter().append("text")
                .attr("class","label")
                .attr("y",_width/1.75+_fontSize/3)
                .attr("x",_width/2)
                .attr("cursor","pointer")
                .attr("width",_width)
                
                .text(function (d) { return _value2+"/"+_minValue2 /*Math.round((_value-_minValue)/(_maxValue-_minValue)*100) + "%"*/ })
                .style("font-size",_fontSize/3+"px")
                .on("click",onMouseClick);
				
			

            path.exit().transition().duration(500).attr("x",1000).remove();


            layout(svg);

            function layout(svg) {

			
				function line1(delay) {
					path.datum(endAngle);
                var env=path.transition().duration(_duration)
                    .attrTween("d", arcTween);
					if(delay)
							env.delay(_duration);
				}
				
				function line2(delay) {
					path2.datum(endAngle2);
                    var env= path2.transition().duration(_duration)
                        .attrTween("d", arcTween2);
						if(delay)
							env.delay(_duration);
				}
				
				function line3(delay) {
					path3.datum(endAngle3);
                    var env= path3.transition().duration(_duration)
                        .attrTween("d", arcTween3);
						if(delay)
							env.delay(_duration);
				}
				
				function line4(delay) {
					path4.datum(endAngle4);
                    var env= path4.transition().duration(_duration)
                        .attrTween("d", arcTween4);
						if(delay)
							env.delay(_duration);
				}
                var ratio=(_value-_minValue)/(_maxValue-_minValue);
                var endAngle=Math.min(360*ratio,360);
                endAngle *= Math.PI/180;
				
				var subRation = ratio-1;
				
				var endAngle2=Math.min(360*(subRation>0?ratio-1:0),360);

				endAngle2 *= Math.PI/180;
				
				var ratio2=(_value2-_minValue2)/(_maxValue2-_minValue2);
				var subRation2 = ratio2-1;
				
				var endAngle3=Math.min(360*ratio2,360);
                endAngle3 *= Math.PI/180;
				
				var endAngle4=Math.min(360*(subRation2>0?ratio2-1:0),360);

				endAngle4 *= Math.PI/180;
				
				
				if(ratio>1) {				
                line1();
				line2(true);

				}
				else if(ratio<1) {
					line2();
					line1(true);

				}
				
				if(ratio2>1) {				
				
				line3();
				line4(true);
				}
				else if(ratio2<1) {
					
					line4();
					line3(true);
				}

                label.datum(Math.round(ratio*100));
                label.transition().duration(_duration*(ratio>1?1:0.5))
                    .tween("text",labelTween);
					
					label2.datum(Math.round(ratio2*100));
                label2.transition().duration(_duration*(ratio2>1?1:0.5))
                    .tween("text",labelTween2);
					
            }

        });

        function onMouseClick(d) {
            if (typeof _mouseClick == "function") {
                _mouseClick.call();
            }
        }
    }

    function labelTween(a) {
        var i = d3.interpolate(_currentValue, a);
        _currentValue = i(0);

        return function(t) {
            _currentValue = i(t);
            this.textContent = _value+"/"+_maxValue;// Math.round(i(t)) + "%";
        }
    }
	
	function labelTween2(a) {
        var i = d3.interpolate(_currentValue2, a);
        _currentValue2 = i(0);

        return function(t) {
            _currentValue2 = i(t);
            this.textContent = _value2+"/"+_maxValue2;// Math.round(i(t)) + "%";
        }
    }

    function arcTween(a) {
        var i = d3.interpolate(_currentArc, a);

        return function(t) {
            _currentArc=i(t);
            return _arc.endAngle(i(t))();
        };
    }

    function arcTween2(a) {
        var i = d3.interpolate(_currentArc2, a);

        return function(t) {
			_currentArc2=i(t);
            return _arc2.endAngle(i(t))();
        };
    }
	
	function arcTween3(a) {
        var i = d3.interpolate(_currentArc3, a);

        return function(t) {
			_currentArc3=i(t);
            return _arc3.endAngle(i(t))();
        };
    }
	
	function arcTween4(a) {
        var i = d3.interpolate(_currentArc4, a);

        return function(t) {
			_currentArc4=i(t);
            return _arc4.endAngle(i(t))();
        };
    }


    function measure() {
        _width=_diameter - _margin.right - _margin.left - _margin.top - _margin.bottom;
        _height=_width;
        _fontSize=_width*.2;
        _arc2.outerRadius(_width/2);
        _arc2.innerRadius(_width/2 * .85);
        _arc.outerRadius(_width/2 * .85);
        _arc.innerRadius(_width/2 * .85 - (_width/2 * .15));
		
		_arc4.outerRadius(_width/3);
        _arc4.innerRadius(_width/3 * .85);
        _arc3.outerRadius(_width/3 * .85);
        _arc3.innerRadius(_width/3 * .85 - (_width/3 * .15));
    }


    component.render = function() {
        measure();
        component();
        return component;
    }

    component.value = function (_) {
        if (!arguments.length) return _value;
        _value = [_];
        _selection.datum([_value]);
        return component;
    }

	    component.value2 = function (_) {
        if (!arguments.length) return _value2;
        _value2 = [_];
        _selection.datum([_value2]);
        return component;
    }

    component.margin = function(_) {
        if (!arguments.length) return _margin;
        _margin = _;
        return component;
    };
	
	component.fontSize = function(_) {
        if (!arguments.length) return _fontSize;
        _fontSize = [_];
        return component;
    };

    component.diameter = function(_) {
        if (!arguments.length) return _diameter
        _diameter =  _;
        return component;
    };

    component.minValue = function(_) {
        if (!arguments.length) return _minValue;
        _minValue = _;
        return component;
    };
	
	    component.minValue2 = function(_) {
        if (!arguments.length) return _minValue2;
        _minValue2 = _;
        return component;
    };

    component.maxValue = function(_) {
        if (!arguments.length) return _maxValue;
        _maxValue = _;
        return component;
    };
	
	    component.maxValue2 = function(_) {
        if (!arguments.length) return _maxValue2;
        _maxValue2 = _;
        return component;
    };

    component.label = function(_) {
        if (!arguments.length) return _label;
        _label = _;
        return component;
    };

    component._duration = function(_) {
        if (!arguments.length) return _duration;
        _duration = _;
        return component;
    };

    component.onClick = function (_) {
        if (!arguments.length) return _mouseClick;
        _mouseClick=_;
        return component;
    }

    return component;

}
