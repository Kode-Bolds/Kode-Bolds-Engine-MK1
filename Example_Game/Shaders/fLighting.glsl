#version 330
 
uniform sampler2D s_texture;

uniform vec4 ambientFactor;
uniform vec4 diffuseFactor;
uniform vec4 specularFactor;
uniform float specularPower;
uniform vec4[100] lightPositions;
uniform vec4[100] lightColours;
uniform int numberOfLights;
uniform vec4 eyePosition;

in vec2 texCoord;
in vec3 normal;
in vec4 surfacePosition;

out vec4 FragColour;

void main()
{	
	vec4 ambient = vec4(0.1, 0.1, 0.1, 1.0);
	vec4 texColour = texture2D( s_texture, texCoord );

	vec3 viewDir = normalize(eyePosition - surfacePosition).xyz;
	
	vec4 finalColour = ambient * ambientFactor;
	for (int i = 0; i < numberOfLights; ++i)
	{
		vec3 lightDir = normalize(lightPositions[i] - surfacePosition).xyz; 
		float diffuse = max(dot(lightDir, normal), 0.0);
		vec3 reflection = normalize(reflect(-lightDir, normal));
		float specular = pow(max(0.0, dot(viewDir, reflection)), specularPower);
		finalColour += clamp(((diffuse * diffuseFactor) + (specular * specularFactor)) * lightColours[i], 0.0, 1.0);
	}
	
	FragColour = finalColour * texColour;  
	
}