var state = false;

function Update() {
    if (Input.GetKeyDown("f1"))
        state = !state;
}

function OnPreRender() {
    if (state)
        GL.wireframe = true;
}

function OnPostRender() {
    if (state)
        GL.wireframe = false;
}