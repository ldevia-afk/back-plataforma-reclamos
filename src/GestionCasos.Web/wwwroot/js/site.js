(function () {
    "use strict";

    var toggle = document.getElementById("gc-sidebar-toggle");
    var sidebar = document.getElementById("gc-sidebar");
    if (toggle && sidebar) {
        toggle.addEventListener("click", function () {
            sidebar.classList.toggle("gc-collapsed");
        });
    }

    // --- Selector de cliente/sucursales en Cases/Create ---
    var clientSelect = document.getElementById("client-select");
    if (clientSelect) {
        var branchGroups = document.querySelectorAll("[data-branch-group]");

        function refreshBranchVisibility() {
            var selectedClientId = clientSelect.value;
            branchGroups.forEach(function (group) {
                var isMatch = group.getAttribute("data-branch-group") === selectedClientId;
                group.hidden = !isMatch;
                group.querySelectorAll("input[type=checkbox]").forEach(function (cb) {
                    cb.disabled = !isMatch;
                    if (!isMatch) cb.checked = false;
                });
            });
        }

        clientSelect.addEventListener("change", refreshBranchVisibility);
        refreshBranchVisibility();

        document.querySelectorAll("[data-select-all-branches]").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var group = document.querySelector('[data-branch-group="' + btn.getAttribute("data-select-all-branches") + '"]');
                if (!group) return;
                group.querySelectorAll("input[type=checkbox]:not(:disabled)").forEach(function (cb) {
                    cb.checked = true;
                });
            });
        });
    }

    // --- "Seleccionar todos" por tabla: un checkbox en el header tilda/destilda
    // todos los checkboxes de esa misma tabla (clase gc-case-checkbox). ---
    document.querySelectorAll(".gc-select-all-in-table").forEach(function (headerCb) {
        var table = headerCb.closest("table");
        if (!table) return;
        headerCb.addEventListener("change", function () {
            table.querySelectorAll(".gc-case-checkbox").forEach(function (cb) {
                cb.checked = headerCb.checked;
            });
        });
    });

    // --- Filtro remoto con debounce: <input data-debounce-submit="400"> envía su
    // formulario (GET) solo, sin apretar ningún botón, cuando la persona deja de
    // escribir. Como es un submit real, el filtrado y el paginado los resuelve el
    // servidor (no se trae toda la tabla al navegador para filtrarla ahí). ---
    document.querySelectorAll("[data-debounce-submit]").forEach(function (input) {
        var delay = parseInt(input.getAttribute("data-debounce-submit"), 10) || 400;
        var timer = null;
        input.addEventListener("input", function () {
            clearTimeout(timer);
            timer = setTimeout(function () {
                if (input.form) input.form.submit();
            }, delay);
        });
    });
})();
