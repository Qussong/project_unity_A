mergeInto(LibraryManager.library, {
    AIT_Vibrate: function(type) {
        var typeMap = {
            0: "softMedium",  // SoftMedium
            1: "basicMedium", // BasicMedium
            2: "error",       // Error
            3: "wiggle"       // Wiggle
        };
        var hapticType = typeMap[type] || "softMedium";

        if (typeof generateHapticFeedback === 'function') {
            generateHapticFeedback({ type: hapticType });
        }
    }
});
