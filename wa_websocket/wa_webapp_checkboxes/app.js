//const { createWebSocketStream } = require("ws");

const ws = new WebSocket('ws://tlacitka.thastertyn.xyz');

let checkboxesDiv;
ws.onopen = () => {
     checkboxesDiv = document.getElementById('checkboxes');
    console.log('Connected to the WebSocket server');
};

ws.onmessage = (event) => {
    const data = JSON.parse(event.data);
    if (data.type === 'initialData') {
        renderCheckboxes(data.data);
    } else if (data.type === 'updateData') {
        console.log("updatedata")
        renderCheckboxes(data.data);
    }
};

// Render entities as toggleable buttons
function renderCheckboxes(entities) {
    checkboxesDiv.innerHTML = ''; 
    // Clear previous buttons
    entities.forEach(entity => {


        const wrapperDiv = document.createElement('div');
        wrapperDiv.className = "checkbox-wrapper-14";

        const checkbox = document.createElement('input');
        checkbox.type = "checkbox";
        checkbox.className = 'switch';
        checkbox.checked = entity.state == 'true';
        checkbox.id = entity.id;

        checkbox.onclick = () => {
            const newState = checkbox.checked; // Toggle state
            ws.send(JSON.stringify({ type: 'changeState', id: entity.id, state: `${newState}` }));
        };
        
        const label = document.createElement("label");
        label.setAttribute("for",entity.id);
        label.className = "switch-label";
        label.innerHTML = `${entity.name}`;

        
        wrapperDiv.appendChild(checkbox);
        wrapperDiv.appendChild(label);

    
        checkboxesDiv.appendChild(wrapperDiv);
    });
}
