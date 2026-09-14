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

    // --- Columnas de las tablas de resultados: se pueden ampliar/reducir arrastrando
    // el borde derecho de cada encabezado. Si la tabla vive dentro de un modal, el
    // modal también se agranda para que la columna redimensionada entre sin recortarse. ---
    document.querySelectorAll("table.gc-table").forEach(function (table) {
        var headers = Array.from(table.querySelectorAll("thead th"));
        if (headers.length < 2) return;

        if (!table.parentElement.classList.contains("gc-table-wrapper")) {
            var wrapper = document.createElement("div");
            wrapper.className = "gc-table-wrapper";
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
        }

        headers.forEach(function (th) {
            th.style.position = "relative";
            var handle = document.createElement("span");
            handle.className = "gc-col-resize-handle";
            th.appendChild(handle);

            handle.addEventListener("mousedown", function (e) {
                e.preventDefault();
                // Al primer arrastre "congelamos" el ancho actual de cada columna (para que
                // el resto no salte) y recién ahí pasamos la tabla a table-layout: fixed.
                if (table.style.tableLayout !== "fixed") {
                    headers.forEach(function (h) { h.style.width = h.offsetWidth + "px"; });
                    table.style.tableLayout = "fixed";
                }

                var startX = e.pageX;
                var startWidth = th.offsetWidth;
                handle.classList.add("gc-resizing");
                document.body.style.userSelect = "none";

                function onMove(ev) {
                    th.style.width = Math.max(50, startWidth + (ev.pageX - startX)) + "px";

                    var modalBox = table.closest(".gc-modal");
                    if (modalBox) {
                        var needed = table.scrollWidth + 56;
                        if (needed > modalBox.getBoundingClientRect().width) {
                            modalBox.style.maxWidth = needed + "px";
                        }
                    }
                }
                function onUp() {
                    handle.classList.remove("gc-resizing");
                    document.body.style.userSelect = "";
                    document.removeEventListener("mousemove", onMove);
                    document.removeEventListener("mouseup", onUp);
                }
                document.addEventListener("mousemove", onMove);
                document.addEventListener("mouseup", onUp);
            });
        });
    });
})();
