window.initProductRails = function initProductRails(root) {
    const scope = root || document;
    const rails = scope.querySelectorAll("[data-pr-rail]");
    if (!rails.length) return;

    rails.forEach((rail) => {
        if (rail.dataset.prBound === "1") return;

        const viewport = rail.querySelector("[data-pr-viewport]");
        const leftBtn = rail.querySelector("[data-pr-prev]");
        const rightBtn = rail.querySelector("[data-pr-next]");

        if (!viewport || !leftBtn || !rightBtn) return;
        rail.dataset.prBound = "1";

        // Per-rail optional config
        const scrollRatioRaw = rail.dataset.prScrollRatio;
        const edgeThresholdRaw = rail.dataset.prEdgeThreshold;

        const scrollRatio = Number.isFinite(Number(scrollRatioRaw))
            ? Math.min(Math.max(Number(scrollRatioRaw), 0.1), 1)
            : 0.85;

        const edgeThreshold = Number.isFinite(Number(edgeThresholdRaw))
            ? Math.max(Number(edgeThresholdRaw), 0)
            : 1;

        function updateButtons() {
            const maxScrollLeft = viewport.scrollWidth - viewport.clientWidth;

            // If it can't scroll, hide both
            if (maxScrollLeft <= 0) {
                leftBtn.classList.add("is-hidden");
                rightBtn.classList.add("is-hidden");
                return;
            }

            const atStart = viewport.scrollLeft <= edgeThreshold;
            const atEnd = viewport.scrollLeft >= maxScrollLeft - edgeThreshold;

            leftBtn.classList.toggle("is-hidden", atStart);
            rightBtn.classList.toggle("is-hidden", atEnd);
        }

        function scrollByPage(direction) {
            const amount = Math.round(viewport.clientWidth * scrollRatio) * direction;
            viewport.scrollBy({ left: amount, behavior: "smooth" });
        }

        leftBtn.addEventListener("click", () => scrollByPage(-1));
        rightBtn.addEventListener("click", () => scrollByPage(1));

        viewport.addEventListener("scroll", () => requestAnimationFrame(updateButtons));
        window.addEventListener("resize", updateButtons);

        if ("ResizeObserver" in window) {
            const resizeObserver = new ResizeObserver(() => updateButtons());
            resizeObserver.observe(viewport);
            const track = rail.querySelector("[data-pr-track]");
            if (track) resizeObserver.observe(track);
        }

        requestAnimationFrame(updateButtons);
        setTimeout(updateButtons, 0);
    });
};
