// site-datetime.js

$(function () {
	var dateFormat = 'yyyy-mm-dd';
	var datetimeFormat = 'yyyy-mm-dd hh:ii';
	var startDate = '2000-01-01';
	var timezoneOffset = 0;
	var timezoneName = null;

	// Extension functions
	$.fn.sitedatetime = function (options, value) {
		if (options === 'check') {
			// Checking option
			return dateFormat;
		}
		else if (options === 'tz-offset' && value !== '') {
			// Returning option, only needed because I cant get defaults to work
			if (value !== undefined)
				timezoneOffset = value;

			return timezoneOffset;
		}
		else if (options === 'tz-name' && value !== '') {
			// Returning option, only needed because I cant get defaults to work
			if (value !== undefined)
				timezoneName = value;

			return timezoneName;
		}
		else {
			// Initialise the extension - not working though
			var settings = $.extend({}, $.fn.sitedatetime.defaults, options);
			return this;
		}
	};

	if ($.widget !== undefined) {
		$.widget("alcazar.datetime", {
			// Default options.
			options: {
				value: 0
			},

			_create: function () {
			}
		});
	}

	// Plugin defaults – added as a property on our plugin function.
	$.fn.sitedatetime.defaults = {
		timezoneOffset: 0,
		timezoneName: null,
	};

	// Initialise the timezone cookies
	setTimezoneCookie();
	getTimezoneOffset();
});

//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//
// region timezones
//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//\\//

// Set the timezone cookie
// The cookie is set by the browser, and used by the server to convert date/time from UTC to the user local timezone
function setTimezoneCookie() {
	if (Cookies) {
		var timezone_offset = "tz-offset";
		var timezone_name = "tz-name";

		// if the timezone cookie not exists create one.
		if (!Cookies.get(timezone_offset)) {
			// create a new timezone-offset cookie 
			Cookies.set(timezone_offset, new Date().getTimezoneOffset());

			var timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;
			if (timezone === undefined)
				timezone = moment.tz.guess();

			// create a new timezone-name cookie 
			Cookies.set(timezone_name, timezone);

			// dont re-load the page, too dangerous
			// location.reload();
		}
		// if the current timezone and the one stored in cookie are different
		// then store the new timezone in the cookie and refresh the page.
		else {
			var storedOffset = parseInt(Cookies.get(timezone_offset));
			var currentOffset = new Date().getTimezoneOffset();

			// user may have changed the timezone
			if (storedOffset !== currentOffset) {
				Cookies.set(timezone_offset, currentOffset);
				// dont re-load the page, too dangerous
				// location.reload();
			}
		}
	}
}

// Set any date/time messages to show the local timezone (e.g. UTC+2)
function getTimezoneOffset() {
	if ($.fn.sitedatetime !== undefined) {
		var tzBanners = $('.tz-offset');

		// Get the timezone name
		var timezoneName = getTimezoneName();
		if (timezoneName !== null) {
			// We have a timezone name, set it as title to all '.date-tz-local'
			var tzTitle = $('.date-tz-local');
			tzTitle.attr('title', timezoneName);

			// Set it as html to all '.tz-offset'
			tzBanners.html(timezoneName);
		}
		else {
			// Despite all endeavours, we dont have a timezone name, use the timezone offset
			var hours = getTimezoneHours();

			// Set the timezone offset (wherever this is still used, instead of the newer datetime display/edit templates)
			if (hours > 0)
				tzBanners.html('UTC+' + hours);
			else
				tzBanners.html('UTC-' + -hours);
		}
	}
}

// Get the timezone name
function getTimezoneName() {
	// Option 1 - the name set by the server
	var timezoneName = $.fn.sitedatetime('tz-name');
	// Option 2 - the name set by newer browsers
	if (timezoneName === null)
		timezoneName = Intl.DateTimeFormat().resolvedOptions().timeZone;
	// Option 3 - the name set by moment.js
	if (timezoneName === null)
		timezoneName = moment.tz.guess();

	return timezoneName;
}

// Get the timezone offset in hours
function getTimezoneHours() {
	// Option 1 - the offset set by the server
	var timezoneOffset = $.fn.sitedatetime('tz-offset');
	// Option 2 - the name set by browsers
	if (timezoneOffset === null)
		timezoneOffset = new Date().getTimezoneOffset();

	return -timezoneOffset / 60;
}