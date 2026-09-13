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
})();
