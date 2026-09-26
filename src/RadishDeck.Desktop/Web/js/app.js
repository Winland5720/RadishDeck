const sections = [
    'Главная',
    'Редактор',
    'Логи',
    'Клиенты',
    'Настройки'
];

let current = 'Главная';
let zoom = .5;

const CANVAS_WIDTH = 1920;
const CANVAS_HEIGHT = 1080;

let editorRenderer = null;

let elements = [];
let selectedId = null;

let server = {
    status: 'Stopped',
    ipAddress: '127.0.0.1',
    port: 5187,
    addresses: []
};

const nav = document.getElementById('nav');
const app = document.getElementById('app');


function send(type, payload = {}) {
    radishBridge.send({
        type,
        payload
    });
}


function renderNav() {
    nav.innerHTML = sections
        .map(section => `
            <button
                class="${section === current ? 'active' : ''}"
                data-s="${section}">
                ${section}
            </button>
        `)
        .join('');

    nav.querySelectorAll('button').forEach(button => {
        button.onclick = () => {
            current = button.dataset.s;

            if (current === 'Редактор') {
                send("window.maximize");
            } else {
                send("window.restore");

            }

            renderNav();
            render();

            send('navigation', {
                section: current
            });
        };
    });
}


function render() {
    app.classList.remove('editor-mode');

    if (current === 'Редактор') {
        editor();
        return;
    }

    if (current === 'Главная') {
        home();
        return;
    }

    app.innerHTML = `
        <h1 class="title">${current}</h1>

        <div class="card muted">
            Раздел подготовлен для следующего этапа RadishDeck.
        </div>
    `;
}


function home() {
    const running = server.status === 'Running';

    const addresses = server.addresses?.length
        ? server.addresses
        : [server.ipAddress];

    app.innerHTML = `
        <h1 class="title">Главная</h1>

        <div class="cards">

            <div class="card">

                <div class="muted">
                    SERVER STATUS
                </div>

                <div class="metric">
                    ${server.status}
                </div>

                <p class="muted">
                    ${server.message || ''}
                </p>

                <label>
                    Bind address

                    <select
                        id="ip"
                        ${server.canEdit ? '' : 'disabled'}>

                        ${addresses
                            .map(address => `
                                <option
                                    ${address === server.ipAddress ? 'selected' : ''}>
                                    ${address}
                                </option>
                            `)
                            .join('')}

                    </select>
                </label>

                <label>
                    Port

                    <input
                        id="port"
                        type="number"
                        min="1"
                        max="65535"
                        value="${server.port}"
                        ${server.canEdit ? '' : 'disabled'}>
                </label>

                <div>
                    <button
                        id="start"
                        ${running || !server.canEdit ? 'disabled' : ''}>
                        Start
                    </button>

                    <button
                        id="restart"
                        ${server.canRestart ? '' : 'disabled'}>
                        Restart
                    </button>

                    <button
                        id="stop"
                        ${server.canStop ? '' : 'disabled'}>
                        Stop
                    </button>
                </div>

                <p>
                    <button
                        id="open"
                        ${running ? '' : 'disabled'}>
                        Открыть пульт
                    </button>

                    <button
                        id="copy"
                        ${running ? '' : 'disabled'}>
                        Копировать адрес
                    </button>
                </p>

                <p class="muted">
                    ${server.accessUrl || 'Server stopped'}
                </p>

            </div>


            <div class="card">

                <div class="muted">
                    CANVAS PROFILE
                </div>

                <div class="metric">
                    Desktop
                </div>

                <p class="muted">
                    1920 × 1080
                </p>

            </div>


            <div class="card">

                <div class="muted">
                    ACTIVITY
                </div>

                <div class="metric">
                    0
                </div>

                <p class="muted">
                    Recent events
                </p>

            </div>

        </div>
    `;


    document.getElementById('start').onclick = () => {
        send('server.start', {
            ipAddress: document.getElementById('ip').value,
            port: document.getElementById('port').value
        });
    };


    document.getElementById('stop').onclick = () => {
        send('server.stop');
    };


    document.getElementById('restart').onclick = () => {
        send('server.restart');
    };


    document.getElementById('open').onclick = () => {
        send('server.openWebRuntime');
    };


    document.getElementById('copy').onclick = () => {
        send('server.copyUrl');
    };
}

function fitDesignerCanvas() {
    const canvasStage = document.getElementById('canvas-stage');
    const canvasWrap = document.querySelector('.canvas-wrap');
    const zoomValue = document.getElementById('zoom-value');

    if (!canvasStage || !canvasWrap || !zoomValue || !editorRenderer) {
        return;
    }

    const availableWidth = canvasWrap.clientWidth - 40;
    const availableHeight = canvasWrap.clientHeight - 40;

    zoom = Math.min(
        availableWidth / CANVAS_WIDTH,
        availableHeight / CANVAS_HEIGHT,
        1
    );

    zoomValue.textContent = Math.round(zoom * 100) + '%';

    canvasStage.style.width = (CANVAS_WIDTH * zoom) + 'px';
    canvasStage.style.height = (CANVAS_HEIGHT * zoom) + 'px';

    editorRenderer.setScale(zoom);
}



function editor() {
    app.classList.add('editor-mode');

    app.innerHTML = `
        <h1 class="title">
            Редактор

            <span
                class="muted"
                style="font-size:14px">
                EDIT MODE · Desktop 1920×1080 · zoom <span id="zoom-value">50%</span>
            </span>
        </h1>


        <div class="grid">

            <aside class="panel">

                <h3>
                    СТРАНИЦЫ
                </h3>

                <div class="row">
                    Main
                </div>


                <h3 style="margin-top:24px">
                    СЛОИ
                </h3>

                <div
                    id="layers"
                    class="muted">

                    ${
                        elements.length
                            ? elements
                                .map(element => `
                                    <div>
                                        ${
                                            element.content?.text
                                            || element.name
                                            || element.type
                                            || element.id
                                        }
                                    </div>
                                `)
                                .join('')
                            : 'Canvas empty'
                    }

                </div>

            </aside>
                    

         <section class="canvas-wrap">
            <div class="device-frame">
              <div id="canvas-stage" class="canvas-stage">
                 <div id="canvas" class="canvas"></div>
              </div>
            </div>        
        </section>


            <aside class="panel">

                <h3>
                    СВОЙСТВА
                </h3>

                <div
                    id="properties"
                    class="muted">
                    Выберите элемент на Canvas
                </div>


                <h3>
                    ЭЛЕМЕНТЫ
                </h3>

                <button
                    id="add"
                    class="row"
                    style="
                        color:white;
                        width:100%;
                        text-align:left;
                    ">
                    ＋ Button
                </button>

            </aside>

        </div>
    `;


    const canvas = document.getElementById('canvas');
    
    const canvasStage = document.getElementById('canvas-stage');
    const canvasWrap = document.querySelector('.canvas-wrap');
    
    const availableWidth = canvasWrap.clientWidth - 40;
    const availableHeight = canvasWrap.clientHeight - 40;          

     zoom = Math.min(
        availableWidth / CANVAS_WIDTH,
        availableHeight / CANVAS_HEIGHT,
        1
    );
    
    document.getElementById('zoom-value').textContent =
        Math.round(zoom * 100) + '%';

    canvasStage.style.width = (CANVAS_WIDTH * zoom) + 'px';
    canvasStage.style.height = (CANVAS_HEIGHT * zoom) + 'px';

    editorRenderer = new RadishSharedRenderer({
        root: canvas,
        canvas: {
            width: CANVAS_WIDTH,
            height: CANVAS_HEIGHT
        }
    });

    editorRenderer.setScale(zoom);
    editorRenderer.render(elements);


    canvas
        .querySelectorAll('.rd-element')
        .forEach(domElement => {

            const element = elements.find(
                item => item.id === domElement.dataset.elementId
            );

            const layout = element.layout;


            domElement.classList.add('element');


            domElement.onclick = event => {
                event.stopPropagation();

                selectedId = element.id;

                render();
            };


            let startX = 0;
            let startY = 0;

            let dragging = false;
            let activePointer = -1;


            const finishDrag = () => {
                if (!dragging) {
                    return;
                }

                dragging = false;

                domElement.onpointermove = null;

                if (domElement.releasePointerCapture) {
                    try {
                        domElement.releasePointerCapture(activePointer);
                    }
                    catch {
                        // Pointer capture may already be released.
                    }
                }

                if (
                    Number.isFinite(layout.x)
                    && Number.isFinite(layout.y)
                ) {
                    send('designer.elementChanged', {
                        id: element.id,
                        x: layout.x,
                        y: layout.y
                    });
                }
            };


            domElement.onpointerdown = event => {
                event.preventDefault();
                event.stopPropagation();

                selectedId = element.id;

                dragging = true;
                activePointer = event.pointerId;

                startX = event.clientX;
                startY = event.clientY;

                domElement.setPointerCapture?.(
                    event.pointerId
                );
            };


            domElement.onpointermove = event => {
                if (
                    !dragging
                    || event.pointerId !== activePointer
                ) {
                    return;
                }


                const deltaX =
                    (event.clientX - startX) / zoom;

                const deltaY =
                    (event.clientY - startY) / zoom;


                layout.x = Math.max(
                    0,
                    Math.min(
                        1920 - layout.width,
                        layout.x + deltaX
                    )
                );


                layout.y = Math.max(
                    0,
                    Math.min(
                        1080 - layout.height,
                        layout.y + deltaY
                    )
                );


                startX = event.clientX;
                startY = event.clientY;


                domElement.style.left =
                    layout.x + 'px';

                domElement.style.top =
                    layout.y + 'px';
            };


            domElement.onpointerup = finishDrag;
            domElement.onpointercancel = finishDrag;


            if (element.id === selectedId) {
                domElement.classList.add('selected');
            }
        });


    const selected = elements.find(
        element => element.id === selectedId
    );


    if (selected) {
        const layout = selected.layout;


        document.getElementById('properties').innerHTML = `
            <div class="row">

                <b>
                    ${
                        selected.content?.text
                        || selected.name
                        || selected.type
                        || 'Element'
                    }
                </b>

                <br>

                <span class="muted">
                    Id: ${selected.id}
                    <br>

                    Type: ${selected.type}
                    <br>

                    X: ${layout.x.toFixed(1)}
                    ·
                    Y: ${layout.y.toFixed(1)}
                    <br>

                    Width: ${layout.width}
                    ·
                    Height: ${layout.height}
                </span>

            </div>
        `;
    }


    document.getElementById('add').onclick = () => {
        send('designer.addElement');
    };
}


radishBridge.onMessage(message => {

    if (message.type === 'renderElements') {

        elements = message.payload.elements || [];

        selectedId =
            message.payload.selectedId
            || selectedId;

        render();
    }

    else if (message.type === 'server.state') {

        server = message.payload;

        render();
    }

    else if (message.type === 'error') {

        console.error(
            'Designer/bridge error:',
            message.payload.message
        );

        document.getElementById('status').textContent =
            'Designer error: '
            + message.payload.message;
    }
});

window.addEventListener('resize', () => {
    if (current === 'Редактор') {
        fitDesignerCanvas();
    }
});


renderNav();
render();

send('server.getState');
send('designer.render');
send('ready');