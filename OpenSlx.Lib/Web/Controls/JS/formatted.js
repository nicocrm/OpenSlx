/*
 * Formatted fields - for number formats
 * For all the fields that need to be formatted, create a 
 * FormattedField object.
 * To assign a value to that field, use FormattedField.setFieldValue.
 * Default FormattedField object uses currency format, subclasses use other formats.
 *
 * Requires jquery.
 *
 * Note that in Saleslogix 6.2.3+ you can use class = currDisplay to get a 
 * similar effect.
 */
 

// add field (an id or object) to the list of formatted field.
// current name of field must be set.
// current value of field must be set (to unformatted value).
function FormattedField(field, numDecimals) {        
    if (arguments.length > 0)
		this.init(field, numDecimals);
}

// Global values (used for formatting): 
// FormattedField.DECIMAL_SEPARATOR
// FormattedField.THOUSAND_SEPARATOR
FormattedField.initGlobals = function() {
    var d = (2.2).toLocaleString();
    FormattedField.DECIMAL_SEPARATOR = d.substr(1, 1);
    d = (22222).toLocaleString();
    FormattedField.THOUSAND_SEPARATOR = d.substr(2, 1);
}

// constructor
// create a hidden field and give it the name from this field, 
// then remove the name from the current field so that it does not get posted.
FormattedField.prototype.init = function(field, numDecimals) {
    var f = getRawObject(field);
    if (!f)
        return;
    if (!FormattedField.list)
        FormattedField.list = new Array();
    FormattedField.list.push(this);
    this.field = f;
    if (typeof (numDecimals) == "undefined")
        numDecimals = 2;
    this._numDecimals = numDecimals;
    var existingHids = $("[name=" + this.field.name + "][type=hidden]");
    var hid;
    if (existingHids.length > 0) {
        // reuse existing hidden control if it exists
        // (useful for ASP.NET updatepanels!)
        hid = existingHids[0];
    } else {
        hid = document.createElement("input");
        hid.type = "hidden";
        hid.name = this.field.name;
    }
    hid.value = this.field.value;
    hid.defaultValue = this.field.defaultValue;

    this.field.name = "";

    var r = this; // bind for closure
    var jqf = $(this.field);
    jqf.focus(function() { r.deformat(); this.select(); });
    jqf.blur(function() { r.reformat() });
    var allowDecimals = this._numDecimals > 0;
    var allowNonNumeric = !!this._allowNonNumeric;
    jqf.keypress(function(e) {
        if (e.which == 13)
        // save the value to the hidden field in case this will be used for a save
            r.setValue(r.field.value);
        if (!allowNonNumeric) {
            return FormattedField.numberPlease(e, allowDecimals);
        }
        return true;
    });
    this.hidden = this.field.parentNode.insertBefore(hid, this.field);
    this.setValue(hid.value);
}

// assign value to a field.
// this makes sure value is formatted correctly.
// pass field (field id or object)
FormattedField.setFieldValue = function(field, value)
{
	var fieldObj = getRawObject(field);
	var fld = FormattedField.findField(fieldObj);
	if(fld)
		fld.setValue(value);
	else
		fieldObj.value = value;
}

// retrieve value from a field.
// this makes sure value is formatted correctly.
// pass field (field id or object)
FormattedField.getFieldValue = function(field)
{
	var fieldObj = getRawObject(field);
	var fld = FormattedField.findField(fieldObj);
	if(fld)
		return fld.getValue();
	else
		return fieldObj.value;
}

FormattedField.findField = function(fieldObj)
{
	if(!FormattedField.list)
		return null;
	for(var i = 0; i < FormattedField.list.length; i++){
		if(FormattedField.list[i].field == fieldObj)
			return FormattedField.list[i];
	}
	return null;
}

// copy hid value into field
FormattedField.prototype.deformat = function ()
{
	this.field.value = this.hidden.value;
	this.isformatted = false;
}

FormattedField.prototype.reformat = function ()
{
	this.setValue(this.field.value);
}

// override this to format the value
FormattedField.prototype.setValue = function(value) {
    var val = value;
    if (typeof value == "string") {
        val = parseFloat(value.replace(new RegExp("[\\$%\\" + FormattedField.THOUSAND_SEPARATOR + "]", "g"), ""));
    } else if (typeof value != "number") {
        throw "Invalid value for formatted field: " + value;
    }
    if (isNaN(val))
        val = 0;
    this.field.value = FormattedField.formatCurrency(val, this._numDecimals);
    this.hidden.value = val;
    this.isformatted = true;
}

// get the raw value
FormattedField.prototype.getValue = function ()
{
	var val;
	if(this.isformatted){
		val = this.hidden.value;
	}else
		val = this.field.value;
	var ival = parseFloat(val);
	if(isNaN(ival))
		return 0.0;
	else
		return ival;
}

// get the formatted value
FormattedField.prototype.getFormattedValue = function ()
{
	var val;

	if(this.isformatted){
		val = this.field.value;
	} else {
		this.reformat();
		val = this.field.value;
		this.deformat();
	}
	return val;
}

// Default keypress event handlers will prevent non-numeric keys.
// Can be overridden to change that behavior
FormattedField.prototype.onKeyPress = function(e) {
}

/*
 * FormattedFieldPhone - phone number format (us format only at this point)
 * In Saleslogix 6.2.3+, use phoneDisplay class to get a similar effect
 * with less effort.
 */
function FormattedFieldPhone(field)
{
	if(arguments.length > 0)
		this.init(field);
}
FormattedFieldPhone.prototype = new FormattedField();
FormattedFieldPhone.prototype.constructor = FormattedFieldPhone;
FormattedFieldPhone.superclass = FormattedField.prototype;

FormattedFieldPhone.prototype.init = function(field) {
    this._allowNonNumeric = true;
    FormattedFieldPhone.superclass.init.call(this, field);
}

FormattedFieldPhone.prototype.setValue = function(value)
{
	var formattedValue = "";

	if(value.length == 10){
		formattedValue = "(" + value.substring(0, 3) + ") " + 
			value.substring(3, 3) + "-" + value.substring(6, 4);
	} else if(value.length == 7){
		formattedValue = value.substring(0, 3) + "-" + value.substring(3, 4);
	} else {
		formattedValue = value;
	}

	this.field.value = formattedValue;
	this.hidden.value = value;
	this.isformatted = true;
}


/*
 * FormattedFieldPercent - percent format
 */
function FormattedFieldPercent(field)
{
	if(arguments.length > 0)
		this.init(field);
}
FormattedFieldPercent.prototype = new FormattedField();
FormattedFieldPercent.prototype.constructor = FormattedFieldPercent;
FormattedFieldPercent.superclass = FormattedField.prototype;

FormattedFieldPercent.prototype.init = function(field) {
    FormattedFieldPercent.superclass.init.call(this, field);
}

FormattedFieldPercent.prototype.setValue = function(value) {
    var val = parseFloat(value);
    if (isNaN(val))
        val = 0;
    if (val > 1)
        val = val / 100;
    this.field.value = (Math.round(val * 100)) + " %"
    this.hidden.value = val;
    this.isformatted = true;
}

// get the raw value
FormattedFieldPercent.prototype.getValue = function() {
    var val = FormattedFieldPercent.superclass.getValue.call(this);
    if (val > 1)
        return val / 100;
    return val;
}


/*
* FormattedFieldDecimal - numeric format, arbitrary number of decimals
*/
function FormattedFieldDecimal(field, numDecimals) {
    if (arguments.length > 0)
        this.init(field, numDecimals);
}
FormattedFieldDecimal.prototype = new FormattedField();
FormattedFieldDecimal.prototype.constructor = FormattedFieldDecimal;
FormattedFieldDecimal.superclass = FormattedField.prototype;


FormattedFieldDecimal.prototype.init = function(field, numDecimals) {
    FormattedFieldDecimal.superclass.init.call(this, field, numDecimals);    
}

FormattedFieldDecimal.prototype.setValue = function(value) {
    var val = parseFloat(value);
    if (isNaN(val))
        val = 0;
    this.field.value = FormattedField.formatCurrency(val, this._numDecimals, "");
    this.hidden.value = val;
    this.isformatted = true;
}


if(!window.getRawObject){
    window.getRawObject = function(obj) {
        if (typeof obj == "string")
            return document.getElementById(obj);
        return obj;
    }
}

// give amount as String
FormattedField.formatCurrency_wkh = function(amount) {
    if (amount.length <= 3) {
        return amount;
    } else {
        return FormattedField.formatCurrency_wkh(amount.substr(0, amount.length - 3)) +
            FormattedField.THOUSAND_SEPARATOR + amount.substr(amount.length - 3);
    }
}

FormattedField.formatCurrency = function(amount, numDecimals, currencySymbol) {
    var acc = "";
    var b = Math.pow(10, numDecimals);
    if (typeof currencySymbol == "undefined")
        currencySymbol = "$";
    amount = Math.round(amount * b) / b;

    var intpart = Math.floor(amount);
    var floatpart = amount - intpart;

    floatpart = Math.round(floatpart * 100);
    if (floatpart < 10)
        floatpart = "0" + floatpart;

    if (intpart >= 1000) {
        acc = FormattedField.formatCurrency_wkh(intpart.toString());
    } else {
        acc = intpart;
    }

    acc = currencySymbol + acc;
    if (numDecimals > 0) {
        acc += FormattedField.DECIMAL_SEPARATOR + floatpart;
    }

    return acc;
}

// Keypress handler - return false if that wasnt a number
FormattedField.numberPlease = function(evt, allowDecimals) {
    evt = (evt) ? evt : event;

    var charCode = evt.charCode;
    if (!charCode)
        charCode = evt.keyCode;
    
    if (typeof console != "undefined") {
        console.log("FormattedField: Char code: " + charCode + ", allowDecimals: " + allowDecimals);
    }
    // 110: decimal point
    // 40 and under: control chars
    // 44: comma, 46: decimal point
    // 48 to 57: number
    // we allow the comma in case they are on a european locale,
    // but it won't format correctly
    if ((charCode < 48 || charCode > 57) && // 0-9
        charCode > 43 &&  // special keys - arrows, tabs, etc
        (charCode < 96 || charCode > 105) &&  // numbers on the decimal pad
        (!allowDecimals || (charCode != 110 && charCode != 188 && charCode != 190 && 
                charCode != 46 && charCode != 44))) {
        if (allowDecimals) {
            alert("Enter only numbers in this field.");
        } else {
            alert("Enter only whole numbers, without cents.");
        }
        return FormattedField.cancelEvent(evt);
    }
    return true;
}

FormattedField.cancelEvent = function(evt) {
    if (!evt)
        evt = window.event;   // IE
    if (evt.returnValue) {
        evt.returnValue = false;
    } else if (evt.preventDefault) {
        evt.preventDefault();
    }
    return false;
}


FormattedField.initGlobals(); 
    

// This is not needed when scripts are loaded via resource?
//if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded(); 
