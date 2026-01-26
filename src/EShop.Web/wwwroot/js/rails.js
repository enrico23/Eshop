window.railScrollByPage = (el, dir = 1) => {
    if (!el) return;
    const amount = Math.max(200, el.clientWidth * 0.9);
    el.scrollBy({ left: dir * amount, behavior: "smooth" });
};

const railInitTrack = (track, left, right) => {
    if (!track || !left || !right) return;

    // Re-init safety: remove previous listeners for this track
    track._railAbort?.abort();
    track._railAbort = new AbortController();
    const { signal } = track._railAbort;
    track._railResize?.disconnect();
    track._railMutation?.disconnect();

    const update = () => {
        const maxScroll = track.scrollWidth - track.clientWidth;
        const atStart = track.scrollLeft <= 1;
        const atEnd = track.scrollLeft >= maxScroll - 1;

        left.classList.toggle("is-hidden", maxScroll <= 0 || atStart);
        right.classList.toggle("is-hidden", maxScroll <= 0 || atEnd);
    };

    update();
    requestAnimationFrame(update);
    requestAnimationFrame(update);

    window.addEventListener("load", update, { signal });
    track.addEventListener("scroll", update, { passive: true, signal });
    window.addEventListener("resize", update, { signal });

    if (window.ResizeObserver) {
        const ro = new ResizeObserver(() => update());
        ro.observe(track);
        track._railResize = ro;
    }

    if (window.MutationObserver) {
        const mo = new MutationObserver(() => update());
        mo.observe(track, { childList: true, subtree: true });
        track._railMutation = mo;
    }
};

window.railInitButtons = (trackId, leftId, rightId) => {
    const track = document.getElementById(trackId);
    const left = document.getElementById(leftId);
    const right = document.getElementById(rightId);
    railInitTrack(track, left, right);
};

window.railInitAll = () => {
    const rails = document.querySelectorAll(".js-rail");
    rails.forEach((rail) => {
        const track = rail.querySelector(".js-rail-track");
        const left = rail.querySelector(".js-rail-left");
        const right = rail.querySelector(".js-rail-right");
        railInitTrack(track, left, right);
    });
};
