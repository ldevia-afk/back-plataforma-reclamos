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

    // --- Buscador en vivo genérico: <input data-live-search-for="idDeLaTabla"> filtra
    // las filas <tr data-search="..."> de esa tabla a medida que se escribe. ---
    document.querySelectorAll("[data-live-search-for]").forEach(function (input) {
        var table = document.getElementById(input.getAttribute("data-live-search-for"));
        if (!table) return;
        var rows = table.querySelectorAll("tbody tr[data-search]");
        var noResultsId = input.getAttribute("data-no-results-for");
        var noResults = noResultsId ? document.getElementById(noResultsId) : null;

        input.addEventListener("input", function () {
            var query = input.value.trim().toLowerCase();
            var visibleCount = 0;
            rows.forEach(function (row) {
                var match = row.getAttribute("data-search").indexOf(query) !== -1;
                row.hidden = !match;
                if (match) visibleCount++;
            });
            if (noResults) noResults.hidden = visibleCount !== 0;
        });
    });
})();
