if (!Object.prototype.inheritsFrom) {
    Object.prototype.inheritsFrom = function (oSuper) {
        for (sProperty in oSuper) this[sProperty] = oSuper[sProperty];
    }
}

if (!Array.prototype.indexOf) {
    Array.prototype.indexOf = function (item) {
        for (var i = 0; i < this.length; i++) if (this[i] == item) return i;
        return -1;
    }
}

if (!String.prototype.trim) { String.prototype.trim = function () { return this.replace(/^\s+/g, '').replace(/\s+$/g, ''); } }

if (!String.prototype.endsWith) {
    String.prototype.endsWith = function (str) {
        if (str.length > this.length) return false;
        if (this.lastIndexOf(str) + str.length == this.length) return true;
        return false;
    }
}

// Begin - Instantiate ESRI Utility Class
var EsriUtils = new function () {
    var appName = window.navigator.appName.toLowerCase();
    this.isNav = appName.indexOf("netscape") != -1;
    this.isIE = appName.indexOf("microsoft") != -1;

    this.userAgent = navigator.userAgent;
    this.navType = "IE";

    if (!this.isIE) {
        if (this.userAgent.indexOf("Firefox") != -1) this.navType = "Firefox";
        else if (this.userAgent.indexOf("Opera") != -1) this.navType = "Opera";
        else if (this.userAgent.indexOf("Safari") != -1) this.navType = "Safari";
        else if (this.userAgent.indexOf("Netscape") != -1) this.navType = "Netscape";
        else this.navType = "Mozilla";
    }

    if (this.isIE) {
        this.graphicsType = "VML";
        document.writeln("<xml:namespace ns=\"urn:schemas-microsoft-com:vml\" prefix=\"v\"/>\n");
        document.writeln("<style type=\"text/css\"> v\\:* { behavior: url(#default#VML);} </style>\n");
    }
    else this.graphicsType = "NS";

    this.leftButton = 1;
    this.rightButton = 2;

    if (this.isNav) this.rightButton = 3;
    this.mouseWheelUnit = 3;

    if (this.isIE) this.mouseWheelUnit = 120;

    this.KEY_LEFT = 37;
    this.KEY_UP = 38;
    this.KEY_RIGHT = 39;
    this.KEY_DOWN = 40;
    this.KEY_ENTER = 13;
    this.KEY_ESCAPE = 27;

    this.hideElement = function (element) { element.style.display = "none"; }
    this.showElement = function (element) { element.style.display = "block"; }
    this.toggleElement = function (element) { (element.style.display.toLowerCase() == "block" || element.style.display.toLowerCase() == "") ? element.style.display = "none" : element.style.display = "block"; }
    this.moveElement = function (element, left, top) { EsriUtils.setElementStyle(element, "left:" + left + "px; top:" + top + "px;"); }

    this.getXY = function (e) {
        if (this.isIE) return new EsriPoint(window.event.clientX + document.body.scrollLeft + document.documentElement.scrollLeft - 2, window.event.clientY + document.body.scrollTop + document.documentElement.scrollTop - 2);
        else return new EsriPoint(e.pageX, e.pageY);
    }

    this.getEventSource = function (e) {
        if (this.isIE) return window.event.srcElement;
        else return e.target;
    }

    this.stopEvent = function (e) {
        if (this.isIE) {
            window.event.returnValue = false;
            window.event.cancelBubble = true;
        }
        else {
            e.preventDefault();
            e.stopPropagation();
        }
    }

    this.getKeyCode = function (e) {
        if (this.isIE) return window.event.keyCode;
        else return e.keyCode;
    }

    this.getElementBounds = function (e) {
        return new EsriRectangle(this.getStyleValue(e.style.left), this.getStyleValue(e.style.top), this.getStyleValue(e.style.width), this.getStyleValue(e.style.height));
    }

    this.getElementPageBounds = function (e) {
        var eL = eT = 0;
        var elB, etB;
        var eW = e.offsetWidth;
        var eH = e.offsetHeight;

        while (e) {
            elB = 0;
            etB = 0;
            eL += e.offsetLeft;
            eT += e.offsetTop;

            if (e.style && e.style.borderWidth != "") {
                elB = parseInt(e.style.borderWidth);
                etB = parseInt(e.style.borderWidth);
            }
            else if (e.style && e.style.borderLeftWidth != "") {
                elB = parseInt(e.style.borderLeftWidth);
                etB = parseInt(e.style.borderTopWidth);
            }

            if (isNaN(elB)) elB = 0;
            if (isNaN(etB)) etB = 0;

            eL += elB;
            eT += etB;
            e = e.offsetParent;
        }

        return new EsriRectangle(eL, eT, eW, eH);
    }

    this.getPageBounds = function () {
        if (window.innerHeight) return new EsriRectangle(0, 0, window.innerWidth, window.innerHeight);
        else if (document.documentElement.clientHeight) return new EsriRectangle(0, 0, document.documentElement.clientWidth, document.documentElement.clientHeight);
        else if (document.body.clientHeight) return new EsriRectangle(0, 0, document.body.clientWidth, document.body.clientHeight);
        else return new EsriRectangle(0, 0, 0, 0);
    }

    this.isLeftButton = function (e) { return this.getMouseButton(e) == this.leftButton; }

    this.getMouseButton = function (e) {
        if (this.isIE) return window.event.button;
        else return e.which;
    }

    function camelizeStyle(name) {
        var a = name.split("-");
        var s = a[0];
        for (var c = 1; c < a.length; c++) s += a[c].substring(0, 1).toUpperCase() + a[c].substring(1);
        return s;
    }

    this.setElementStyle = function (e, css) {
        if (e && e.style) {
            var es = e.style;
            var ss = css.split(";");
            for (var i = 0; i < ss.length; i++) {
                var s = ss[i].split(":");
                s[0] = s[0].trim();
                if (s[0] == "" || !s[1]) continue;
                eval("es." + camelizeStyle(s[0]) + " = \"" + s[1].trim() + "\"");
            }
        }
    }

    this.getStyleByClassName = function (name) {
        var styleSheets = document.styleSheets;
        name = name.toLowerCase();
        for (var s = 0; s < styleSheets.length; s++) {
            var rules;
            if (this.isIE) rules = styleSheets.item(s).rules;
            else rules = styleSheets.item(s).cssRules;
            for (var i = 0; i < rules.length; i++) {
                if (rules.item(i).selectorText.toLowerCase() == name) return rules.item(i).style;
            }
        }

        return null;
    }

    this.removeElementStyle = function (e, css) {
        if (e) {
            var es = e.style;
            var ss = css.split(";");
            for (var i = 0; i < ss.length; i++) {
                var s = ss[i].split(":");
                s[0] = s[0].trim();
                if (s[0] == "") continue;
                if (this.isIE) es.removeAttribute(camelizeStyle(s[0]));
                else es.removeProperty(s[0]);
            }
        }
    }

    this.setElementOpacity = function (e, o) {
        if (this.isIE) this.setElementStyle(e, "filter:alpha(opacity=" + (o * 100) + ");");
        else this.setElementStyle(e, "-moz-opacity:" + o + "; opacity:" + o + ";");
    }

    this.cloneElementStyle = function (src, target) {
        var ss = src.style;
        var ts = target.style;
        for (var s in ss) try { eval("ts." + s + " = ss." + s + ";"); } catch (e) { };
    }

    this.getElementsByClassName = function (element, className) {
        if (element) {
            var cs = element.getElementsByTagName("*");
            var es = [];
            for (var i = 0; i < cs.length; i++) {
                var c = cs.item(i);
                if (c.className.match(new RegExp("(^|\\s)" + className + "(\\s|$)"))) es.push(c);
            }
            return es;
        }
        else return null;
    }

    this.getStyleValue = function (s) {
        if (typeof s == "number") return s;
        var index = s.indexOf("px");
        if (index == -1) {
            index = s.indexOf("%");
            if (index == -1) return s;
            else return parseInt(s.substring(0, index));
        }
        return parseInt(s.substring(0, index));
    }
}
// End - Instantiate ESRI Utility Class

// Begin - ESRI Point Class
function EsriPoint(x, y) {
    this.x = this.y = null;
    this.reshape(x, y);
}

EsriPoint.prototype.reshape = function (x, y) {
    this.x = x;
    this.y = y;
}

EsriPoint.prototype.offset = function (oX, oY) { return new EsriPoint(this.x + oX, this.y + oY); }
EsriPoint.prototype.equals = function (pt) { return this.x == pt.x && this.y == pt.y; }
EsriPoint.prototype.toString = function () { return "EsriPoint [x = " + this.x + ", y = " + this.y + "]"; }
// End - ESRI Point Class

// Begin - ESRI Rectangle Class
function EsriRectangle(left, top, width, height) {
    this.left = this.top = this.width = this.height = 0;
    this.reshape(left, top, width, height);
}

EsriRectangle.prototype.reshape = function (left, top, width, height) {
    this.left = left;
    this.top = top;
    this.width = width;
    this.height = height;
}

EsriRectangle.prototype.parseStyle = function (style) {
    var a = style.split(";");
    var l = t = w = h = 0;

    for (var i = 0; i < a.length; i++) {
        if (a[i] == "") continue;
        var p = a[i].trim().split(":");
        if (p[0] == "" || p[1] == "") continue;
        switch (p[0]) {
            case "left": l = EsriUtils.getStyleValue(p[1]); break;
            case "top": t = EsriUtils.getStyleValue(p[1]); break;
            case "width": w = EsriUtils.getStyleValue(p[1]); break;
            case "height": h = EsriUtils.getStyleValue(p[1]); break;
        }
    }

    this.reshape(l, t, w, h);
    return this;
}

EsriRectangle.prototype.offset = function (oX, oY) { return new EsriRectangle(this.left + oX, this.top + oY, this.width, this.height); }

EsriRectangle.prototype.reshape = function (left, top, width, height) {
    this.left = left;
    this.top = top;
    this.width = width;
    this.height = height;
    this.center = new EsriPoint(this.left + (this.width / 2), this.top + (this.height / 2));
}

EsriRectangle.prototype.scale = function (factor, scaleCenter) {
    var newWd = this.width * factor;
    var newHt = this.height * factor;
    var rect = new EsriRectangle(this.center.x - (newWd / 2), this.center.y - (newHt / 2), newWd, newHt);

    if (scaleCenter) {
        var x = this.center.x - scaleCenter.x;
        var y = this.center.y - scaleCenter.y;
        var shiftX = x * factor;
        var shiftY = y * factor;
        var moveX = shiftX - x;
        var moveY = shiftY - y;
        rect = rect.offset(moveX, moveY);
    }
    return rect;
}

EsriRectangle.prototype.parseStyle = function (style) {
    var a = style.split(";");
    var l = t = w = h = 0;
    for (var i = 0; i < a.length; i++) {
        if (a[i] == "") continue;
        var p = a[i].trim().split(":");
        if (p[0] == "" || p[1] == "") continue;
        switch (p[0]) {
            case "left": l = EsriUtils.getStyleValue(p[1]); break;
            case "top": t = EsriUtils.getStyleValue(p[1]); break;
            case "width": w = EsriUtils.getStyleValue(p[1]); break;
            case "height": h = EsriUtils.getStyleValue(p[1]); break;
        }
    }
    this.reshape(l, t, w, h);
    return this;
}

EsriRectangle.prototype.equals = function (rect) { return this.left == rect.left && this.top == rect.top && this.width == rect.width && this.height == rect.height; }
EsriRectangle.prototype.toStyle = function () { return "left:" + this.left + "px; top:" + this.top + "px; width:" + this.width + "px; height:" + this.height + "px;"; }
EsriRectangle.prototype.toString = function () { return "EsriRectangle [left = " + this.left + ", top = " + this.top + ", width = " + this.width + ", height = " + this.height + "]"; }
// End - ESRI Rectangle Class

// Begin - EsriPageElement Class
function EsriPageElement(id, element) {
    this.id = id; // this object ID
    this.elementID = (element) ? ((element.id) ? element.id : "element_" + id) : "";
    this.objElement = (element) ? element : null;
    this.bounds = (element) ? EsriUtils.getElementBounds(element) : null;

    this.moveTo = function (left, top) {
        this.bounds.reshape(left, top, this.bounds.width, this.bounds.height);
        EsriUtils.setElementStyle(this.objElement, "left:" + left + "px; top:" + top + "px;");
    }

    this.resize = function (wd, ht) {
        this.bounds.reshape(this.bounds.left, this.bounds.top, wd, ht);
        EsriUtils.setElementStyle(this.objElement, "width:" + wd + "px; height:" + ht + "px;");
    }
}

EsriPageElement.prototype.show = function () { EsriUtils.showElement(this.objElement); }
EsriPageElement.prototype.hide = function () { EsriUtils.hideElement(this.objElement); }
//End - PageElement Class

// Begin - EsriFloatingWindow Class
function EsriFloatingWindow(id, titleText, container, width, height) {
    this.inheritsFrom(new EsriPageElement(id));

    this.title = titleText;
    this.elementID = "element_" + id;
    this.contentElement = null;

    this.collapsed = this.closed = false;
    this.resizable = this.closable = this.movable = true;
    this.collapsable = false;

    this.closeImgUrl = "images/close.png";
    this.resizeImgUrl = "images/resize.png";
    this.expandImgUrl = "images/expand.png";
    this.collapseImgUrl = "images/collapse.png";

    this.minWidth = 200;
    this.minHeight = 40;
    this.maxWidth = 1000;
    this.maxHeight = 800;

    this.closeLabel = "Close";
    this.resizeLabel = "Resize Window";
    this.collapseLabel = "Collapse/Expand";

    this.updateListeners = [];
    this.updateListenerNames = [];

    var titleBar = null;
    var coverElement = null;
    var contentObject = null;
    var mvAct, rzAct, resDiv, resImg, colImg, clsImg, expWd, expHt;
    var resizing = moving = false;
    var startPt = null;
    var scrSz = 20;
    var self = this;

    this.init = function (container, w, h) {
        if (!container) container = document.body;

        coverElement = container.appendChild(document.createElement("div"));
        coverElement.className = "silverlightCover";
        EsriUtils.setElementOpacity(coverElement, 0.5);

        self.objElement = container.appendChild(document.createElement("div"));
        self.objElement.id = self.elementID;
        self.objElement.className = "esriWindow";
        //self.bounds = new EsriRectangle(0, 0, w, h); // Set Content Size instead
        EsriUtils.setElementStyle(self.objElement, "top:0px;left:0px;");

        self.contentElement = document.createElement("iframe"); // Create a browser
        self.contentElement.className = "esriWindowContent";
        self.contentElement.frameBorder = 0; // For IE
        self.contentElement.scrolling = "no"; // For IE
        self.contentElement.setAttribute("frameborder", "0");
        self.contentElement.setAttribute("scrolling", "no");
        EsriUtils.setElementStyle(self.contentElement, "width:" + w + "px;height:" + h + "px;");

        contentObject = new EsriPageElement("winContent", self.contentElement);
        contentObject.bounds = new EsriRectangle(0, 0, w, h);

        var winTable = self.objElement.appendChild(document.createElement("table"));
        winTable.cellPadding = 0; // For IE
        winTable.cellSpacing = 0; // For IE
        winTable.setAttribute("cellpadding", "0");
        winTable.setAttribute("cellspacing", "0");
        winTable.className = "esriWindowTable";

        var tbody = winTable.appendChild(document.createElement("tbody"));
        EsriUtils.setElementStyle(tbody, "border:0px; margin:0px; padding:0px;");

        var titleRow = tbody.appendChild(document.createElement("tr"));
        var titleCell = titleRow.appendChild(document.createElement("td"));
        titleCell.vAlign = "top";

        titleBar = titleCell.appendChild(document.createElement("table"));
        titleBar.className = "esriWindowTitleBar";
        titleBar.onclick = function (e) { EsriUtils.stopEvent(e); self.toFront(); return false; }

        var titleTr = titleBar.appendChild(document.createElement("tbody")).appendChild(document.createElement("tr"));
        var titleTd = titleTr.appendChild(document.createElement("td"));
        titleTd.vAlign = "middle";
        titleTd.align = "left";
        titleTd.width = "100%";
        titleTd.noWrap = true;

        var titleSpan = titleTd.appendChild(document.createElement("span"));
        titleSpan.className = "esriWindowTitleText";
        titleSpan.appendChild(document.createTextNode(self.title));

        var contentRow = tbody.appendChild(document.createElement("tr"));
        var contentCell = contentRow.appendChild(document.createElement("td"));
        contentCell.appendChild(self.contentElement);
        contentCell.vAlign = "top";

        if (self.resizable) {
            resDiv = self.objElement.appendChild(document.createElement("div"));
            resDiv.className = "resizeWindonButton";
            resDiv.onmousedown = processResizeMouseDown;
            resImg = resDiv.appendChild(document.createElement("img"));
            resImg.setAttribute("title", self.resizeLabel);
            resImg.src = self.resizeImgUrl;
        }

        if (self.collapsable) {
            colImg = titleTr.appendChild(document.createElement("td")).appendChild(document.createElement("img"));
            colImg.src = this.collapseImgUrl;
            colImg.onmouseover = function (e) { colImg.style.cursor = "pointer"; }
            colImg.onclick = function (e) { (self.collapsed) ? self.expand() : self.collapse(); return false; }
            colImg.onmouseout = function (e) { colImg.style.cursor = "default"; }
            colImg.setAttribute("title", self.collapseLabel);
        }

        if (self.closable) {
            clsImg = titleTr.appendChild(document.createElement("td")).appendChild(document.createElement("img"));
            clsImg.src = this.closeImgUrl;
            clsImg.onmouseover = function (e) { clsImg.style.cursor = "pointer"; }
            clsImg.onclick = function (e) { (self.closed) ? self.show() : self.hide(); return false; }
            clsImg.onmouseout = function (e) { clsImg.style.cursor = "default"; }
            clsImg.setAttribute("title", self.closeLabel);
        }

        if (self.movable) {
            titleBar.onmousedown = processMoveMouseDown;
            titleBar.style.cursor = "move";
        }

        if (self.collapsed) self.collapse(true);
        if (self.closed) self.hide(true);

        normalize();
    }

    function processResizeMouseDown(e) {
        processMouseDown(e, "resize");
        return false;
    }

    function processMoveMouseDown(e) {
        processMouseDown(e, "move");
        return false;
    }

    function processMouseDown(e, action) {
        if (!EsriUtils.isLeftButton(e)) return;
        document.onmousemove = processMouseMove;
        document.onmouseup = processMouseUp;
        startPt = EsriUtils.getXY(e);
        EsriUtils.stopEvent(e);

        if (action == "resize") {
            resizing = true;
            moving = false;
        }
        else if (action == "move") {
            resizing = false;
            moving = true;
        }

        return false;
    }

    function processMouseMove(e) {
        if (!EsriUtils.isLeftButton(e)) {
            moving = false;
            resizing = false;
            return false;
        }

        var pt = EsriUtils.getXY(e);
        var dx = pt.x - startPt.x;
        var dy = pt.y - startPt.y;

        if (moving) {
            var wl = self.bounds.left + dx;
            var wt = self.bounds.top + dy;
            var l = (wl < 0) ? 0 : wl;
            var t = (wt < 0) ? 0 : wt;
            EsriUtils.setElementStyle(self.objElement, "left:" + l + "px; top:" + t + "px;");
        }
        else if (resizing) {
            var b = self.bounds;
            var cb = contentObject.bounds;
            EsriUtils.setElementStyle(self.contentElement, "width:" + (cb.width + dx) + "px; height:" + (cb.height + dy) + "px;");
            EsriUtils.setElementStyle(self.objElement, "width:" + (b.width + dx) + "px; height:" + (b.height + dy) + "px;");
        }

        EsriUtils.stopEvent(e);
        return false;
    }

    function processMouseUp(e) {
        if (moving || resizing) {
            moving = false;
            resizing = false;

            document.onmousemove = document.onmouseup = null;
            EsriUtils.stopEvent(e);
            updateBounds();
        }

        return false;
    }

    function updateBounds() {
        if (!self.closed) {
            self.bounds = EsriUtils.getElementPageBounds(self.objElement);
            if (!self.collapsed) contentObject.bounds = EsriUtils.getElementPageBounds(self.contentElement);
        }

        for (var i = 0; i < self.updateListenerNames.length; i++) self.updateListeners[self.updateListenerNames[i]](self);
    }

    function normalize() {
        var b = EsriUtils.getElementPageBounds(self.objElement);
        var x = b.left;
        var y = b.top;
        var w = b.width;
        var h = b.height;

        if ((self.minWidth && self.maxWidth) && (b.width < self.minWidth || b.width > self.maxWidth)) {
            w = (b.width < self.minWidth) ? self.minWidth : self.maxWidth;
        }

        if ((self.minHeight && self.maxHeight) && (b.height < self.minHeight || b.height > self.maxHeight)) {
            h = (b.height < self.minHeight) ? self.minHeight : self.maxHeight;
        }

        var pb = EsriUtils.getPageBounds();
        if (w > pb.width) w = pb.width;
        if (h > pb.height) h = pb.height;
        if (x + w > pb.width) x = pb.width - w;
        if (y + h > pb.height) y = pb.height - h;

        if (x != b.left || y != b.top) self.moveTo(x, y);

        if (w != b.width || h != b.height)
            self.resize(w, h);
        else updateBounds();
    }

    function setZ(z) { EsriUtils.setElementStyle(self.objElement, "z-index:" + z + ";"); }

    this.setContentSource = function (source) { self.contentElement.src = source; }

    this.collapse = function () {
        if (self.collapsed || self.closed) return;
        var tb = EsriUtils.getElementPageBounds(titleBar);
        var cb = EsriUtils.getElementPageBounds(self.contentElement);
        EsriUtils.hideElement(self.contentElement);
        if (resDiv) EsriUtils.hideElement(resDiv);

        expWd = tb.width;
        EsriUtils.setElementStyle(titleBar, "width:" + expWd + "px;");

        expHt = cb.height;
        var h = self.bounds.height - expHt;
        EsriUtils.setElementStyle(self.objElement, "height:" + ((h < 0 && EsriUtils.isIE) ? 0 : h) + "px;");

        colImg.src = self.expandImgUrl;
        self.collapsed = true;
        updateBounds();
    }

    this.expand = function () {
        if (!self.collapsed || self.closed) return;
        EsriUtils.showElement(self.contentElement);
        if (resDiv) EsriUtils.showElement(resDiv);
        EsriUtils.removeElementStyle(titleBar, "width;");

        var expHtSet = self.contentElement.style.height == null;
        if (expHtSet) EsriUtils.setElementStyle(self.objElement, "height:" + (self.bounds.height + expHt) + "px;");
        else EsriUtils.removeElementStyle(self.objElement, "height;");

        colImg.src = self.collapseImgUrl;
        self.collapsed = false;
        updateBounds();
    }

    this.show = function (source) {
        if (!self.closed) return;
        EsriUtils.showElement(self.objElement);
        EsriUtils.showElement(coverElement);
        if (source) self.contentElement.src = source;
        self.closed = false;
        updateBounds();
    }

    this.hide = function () {
        if (self.closed) return;
        EsriUtils.hideElement(self.objElement);
        EsriUtils.hideElement(coverElement);
        self.contentElement.src = "";
        self.closed = true;
        updateBounds();
    }

    this.toFront = function (zi) { setZ((zi) ? zi : 999); }
    this.toBack = function (zi) { setZ((zi) ? zi : 111); }

    this.moveTo = function (x, y) {
        if (!self.movable) return;
        var pb = EsriUtils.getPageBounds();
        EsriUtils.setElementStyle(self.objElement, "left:" + ((x < 0 || x > pb.width) ? 0 : x) + "px; top:" + ((y < 0 || y > pb.height) ? 0 : y) + "px;");
        updateBounds();
    }

    this.resize = function (w, h) {
        if (!self.resizable) return;
        var sb = EsriUtils.getElementPageBounds(self.objElement);
        var cb = EsriUtils.getElementPageBounds(self.contentElement);

        var dx = sb.width - cb.width;
        var dy = sb.height - cb.height;

        if (w - dx > 0) {
            EsriUtils.setElementStyle(self.objElement, "width:" + w + "px;");
            EsriUtils.setElementStyle(self.contentElement, "width:" + (w - dx) + "px;");
        }

        if (w - dy > 0) {
            EsriUtils.setElementStyle(self.objElement, "height:" + h + "px;");
            EsriUtils.setElementStyle(self.contentElement, "height:" + (h - dy) + "px;");
        }

        updateBounds();
    }

    this.init(container, width, height);
}

EsriFloatingWindow.prototype.center = function () {
    var page = EsriUtils.getPageBounds();
    var wW = page.width;
    var wH = page.height;
    this.moveTo(Math.round(wW / 2) - Math.round(this.bounds.width / 2), Math.round(wH / 2) - Math.round(this.bounds.height / 2));
}

EsriFloatingWindow.prototype.addUpdateListener = function (name, listener) {
    if (this.updateListenerNames.indexOf(name) == -1) this.updateListenerNames.push(name);
    this.updateListeners[name] = listener;
}

EsriFloatingWindow.prototype.removeUpdateListener = function (name) {
    var index = this.updateListenerNames.indexOf(name);
    if (index != -1) {
        this.updateListenerNames.splice(index, 1);
        this.updateListeners[name] = null;
    }
}

EsriFloatingWindow.prototype.toggleVisibility = function () { this.closed ? this.show() : this.hide(); }
EsriFloatingWindow.prototype.toggleCollapse = function () { this.collapsed ? this.expand() : this.collapse(); }
// End - EsriFloatingWindow Class