#version 330
 
uniform sampler2D s_texture;

uniform vec4 ambientFactor;
uniform vec4 diffuseFactor;
uniform vec4 specularFactor;
uniform float specularPower;

in vec2 texCoord;
in vec3 normal;
in vec3 lightDir;
in vec3 viewDir;

out vec4 FragColour;

void main()
{	
	//vec3 reflectedLight = vec3(normalize(dot(lightDir, normal)));
	//vec3 reflection = normalize(((2.0 * normal) * reflectedLight) - lightDir);

    vec3 reflection = normalize(reflect(lightDir, normal));

	vec4 texColour = texture2D( s_texture, texCoord );
	vec4 lightColour = vec4(0.9, 0.9, 0.9, 1);
   
	vec4 ambient = vec4(0.3, 0.3, 0.3, 1.0);
	float diffuse = max(dot(normal, lightDir), 0.0);
	vec4 diffuseColour = lightColour * diffuse;
	float specular = pow(max(0.0, dot(viewDir, reflection)), specularPower);
	vec4 specularColour = lightColour * specular;

	vec4 finalColour = clamp((ambient + (diffuseColour * diffuseFactor) + (specularColour * specularFactor)), 0.0, 1.0);
   
	FragColour = clamp(finalColour * texColour, 0.0, 1.0);  
}