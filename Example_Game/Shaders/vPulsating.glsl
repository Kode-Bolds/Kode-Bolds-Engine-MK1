#version 330

in vec3 a_Position;
in vec2 a_TexCoord;
in vec3 normal;

uniform float time;
uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProj;

out vec2 texCoord;

void main()
{
	float disp = 0.2 * sin(7 * time * (a_Position.x + a_Position.y));
    vec3 pulsatedPosition = a_Position + (disp * normal);
	gl_Position = vec4(pulsatedPosition,1) * uModel * uView * uProj;
	texCoord = a_TexCoord;
}