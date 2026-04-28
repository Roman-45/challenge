// ── Toast auto-dismiss ──
document.querySelectorAll('.toast').forEach(el => {
    setTimeout(() => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(-12px)';
        setTimeout(() => el.remove(), 300);
    }, 5000);
});

// ── Flatpickr date pickers ──
document.querySelectorAll('.datepicker').forEach(el => {
    flatpickr(el, {
        dateFormat: 'Y-m-d',
        altInput: true,
        altFormat: 'F j, Y',
        maxDate: 'today',
        allowInput: true,
        disableMobile: true
    });
});

document.querySelectorAll('.monthpicker').forEach(el => {
    flatpickr(el, {
        dateFormat: 'F Y',
        altInput: false,
        allowInput: true,
        disableMobile: true,
        plugins: [],
        // Show only month/year selection
        onChange: function(selectedDates, dateStr, instance) {
            if (selectedDates.length > 0) {
                var d = selectedDates[0];
                var months = ['January','February','March','April','May','June',
                              'July','August','September','October','November','December'];
                el.value = months[d.getMonth()] + ' ' + d.getFullYear();
            }
        }
    });
});

// ── Step wizard navigation ──
function goStep(stepNum) {
    // Hide all step panels
    document.querySelectorAll('.step-panel').forEach(p => {
        p.classList.remove('step-panel--active');
    });

    // Show target panel
    var panel = document.getElementById('step' + stepNum);
    if (panel) panel.classList.add('step-panel--active');

    // Update step indicators
    document.querySelectorAll('.step').forEach(s => {
        var sn = parseInt(s.getAttribute('data-step'));
        s.classList.remove('step--active', 'step--done');
        if (sn === stepNum) s.classList.add('step--active');
        else if (sn < stepNum) s.classList.add('step--done');
    });

    // Scroll to top of form
    var card = document.querySelector('.card');
    if (card) card.scrollIntoView({ behavior: 'smooth', block: 'start' });
}
