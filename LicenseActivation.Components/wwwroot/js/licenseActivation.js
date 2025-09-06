// Browser Fingerprinting Service
window.browserFingerprint = {
    // Generate machine fingerprint (consistent across browsers on same machine)
    async generateFingerprint() {
        const fingerprint = {
            // Hardware characteristics (consistent across browsers)
            platform: navigator.platform,
            hardwareConcurrency: navigator.hardwareConcurrency || 0,
            deviceMemory: navigator.deviceMemory || 0,
            maxTouchPoints: navigator.maxTouchPoints || 0,

            // Screen hardware properties (physical display)
            screen: {
                width: screen.width,
                height: screen.height,
                colorDepth: screen.colorDepth,
                pixelDepth: screen.pixelDepth
            },

            // System timezone (machine setting)
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
            timezoneOffset: new Date().getTimezoneOffset(),

            // GPU/Graphics hardware fingerprint
            webgl: this.getMachineWebGLFingerprint(),

            // Audio hardware fingerprint
            audio: await this.getMachineAudioFingerprint(),

            // System fonts (installed on machine, not browser-specific)
            fonts: this.getSystemFonts(),

            // Network hardware characteristics
            connection: this.getNetworkHardwareInfo()
        };

        // Generate hash of all properties
        const fingerprintString = JSON.stringify(fingerprint);
        const hash = await this.generateHash(fingerprintString);

        return {
            hash: hash,
            details: fingerprint,
            raw: fingerprintString
        };
    },

    // Machine WebGL fingerprinting (focuses on hardware GPU characteristics)
    getMachineWebGLFingerprint() {
        try {
            const canvas = document.createElement('canvas');
            const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');

            if (!gl) return 'no-webgl';

            const debugInfo = gl.getExtension('WEBGL_debug_renderer_info');
            const vendor = gl.getParameter(debugInfo?.UNMASKED_VENDOR_WEBGL || gl.VENDOR);
            const renderer = gl.getParameter(debugInfo?.UNMASKED_RENDERER_WEBGL || gl.RENDERER);

            // Focus on hardware capabilities that are consistent across browsers
            return {
                vendor: vendor,
                renderer: renderer,
                maxTextureSize: gl.getParameter(gl.MAX_TEXTURE_SIZE),
                maxViewportDims: gl.getParameter(gl.MAX_VIEWPORT_DIMS),
                maxVertexAttribs: gl.getParameter(gl.MAX_VERTEX_ATTRIBS),
                maxVaryingVectors: gl.getParameter(gl.MAX_VARYING_VECTORS),
                maxFragmentUniforms: gl.getParameter(gl.MAX_FRAGMENT_UNIFORM_VECTORS),
                maxVertexUniforms: gl.getParameter(gl.MAX_VERTEX_UNIFORM_VECTORS)
            };
        } catch (e) {
            return 'webgl-error';
        }
    },

    // Machine audio hardware fingerprinting (focuses on hardware capabilities)
    async getMachineAudioFingerprint() {
        try {
            const audioContext = new (window.AudioContext || window.webkitAudioContext)();
            
            // Get hardware-specific audio characteristics
            const hardwareInfo = {
                sampleRate: audioContext.sampleRate,
                maxChannelCount: audioContext.destination.maxChannelCount,
                numberOfInputs: audioContext.destination.numberOfInputs,
                numberOfOutputs: audioContext.destination.numberOfOutputs,
                channelCount: audioContext.destination.channelCount,
                state: audioContext.state
            };
            
            audioContext.close();
            return JSON.stringify(hardwareInfo);
        } catch (e) {
            return 'audio-error';
        }
    },

    // System fonts detection (machine-specific, installed fonts)
    getSystemFonts() {
        const baseFonts = ['monospace', 'sans-serif', 'serif'];
        const systemFonts = [
            // Windows system fonts
            'Segoe UI', 'Calibri', 'Consolas', 'Microsoft Sans Serif',
            // macOS system fonts
            'San Francisco', 'Helvetica Neue', 'Monaco', 'Menlo',
            // Linux system fonts
            'Ubuntu', 'Liberation Sans', 'DejaVu Sans', 'Noto Sans',
            // Common cross-platform fonts
            'Arial', 'Times New Roman', 'Courier New', 'Verdana', 'Georgia',
            'Tahoma', 'Impact', 'Trebuchet MS', 'Comic Sans MS'
        ];

        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        const text = 'mmmmmmmmmmlli';
        const testSize = '72px';

        const baseSizes = {};
        for (const baseFont of baseFonts) {
            context.font = testSize + ' ' + baseFont;
            baseSizes[baseFont] = context.measureText(text).width;
        }

        const installedFonts = [];
        for (const testFont of systemFonts) {
            let detected = false;
            for (const baseFont of baseFonts) {
                context.font = testSize + ' ' + testFont + ', ' + baseFont;
                const width = context.measureText(text).width;
                if (width !== baseSizes[baseFont]) {
                    detected = true;
                    break;
                }
            }
            if (detected) {
                installedFonts.push(testFont);
            }
        }

        return installedFonts.sort().join(','); // Sort for consistency
    },

    // Network hardware characteristics (focuses on hardware capabilities)
    getNetworkHardwareInfo() {
        const connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
        if (!connection) return 'no-connection-api';

        // Focus on hardware characteristics, not current connection state
        return {
            // Connection type indicates hardware capability
            type: connection.type || 'unknown'
        };
    },


    // Generate SHA-256 hash
    async generateHash(message) {
        const encoder = new TextEncoder();
        const data = encoder.encode(message);
        const hashBuffer = await crypto.subtle.digest('SHA-256', data);
        const hashArray = Array.from(new Uint8Array(hashBuffer));
        return hashArray.map(b => b.toString(16).padStart(2, '0')).join('');
    },

    // Get a simplified machine identifier (machine-specific, not browser-specific)
    getMachineId() {
        const machineFingerprint = {
            platform: navigator.platform,
            hardwareConcurrency: navigator.hardwareConcurrency || 0,
            screen: screen.width + 'x' + screen.height + 'x' + screen.colorDepth,
            timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
            deviceMemory: navigator.deviceMemory || 0
        };

        return btoa(JSON.stringify(machineFingerprint)).replace(/[^a-zA-Z0-9]/g, '').substring(0, 16);
    }
};

// RSA Signature Verification using Web Crypto API
window.rsaVerification = {
    // Convert base64 to ArrayBuffer
    base64ToArrayBuffer(base64) {
        try {
            // Remove any whitespace and newlines
            const cleanBase64 = base64.replace(/[\s\n\r]/g, '');
            const binaryString = atob(cleanBase64);
            const bytes = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                bytes[i] = binaryString.charCodeAt(i);
            }
            return bytes.buffer;
        } catch (error) {
            console.error('Error converting base64 to ArrayBuffer:', error);
            throw error;
        }
    },

    // Import RSA public key from base64-encoded PEM string
    async importPublicKey(publicKeyInput) {
        try {
            // Decode the base64 to get the PEM content
            const decodedPem = atob(publicKeyInput);
            console.log('Decoded PEM content:', decodedPem);
            
            // Extract the base64 key content from between the PEM headers
            const pemLines = decodedPem.split('\n');
            const base64Lines = [];
            let inKey = false;
            
            for (const line of pemLines) {
                if (line.includes('BEGIN PUBLIC KEY') || line.includes('BEGIN RSA PUBLIC KEY')) {
                    inKey = true;
                    continue;
                }
                if (line.includes('END PUBLIC KEY') || line.includes('END RSA PUBLIC KEY')) {
                    break;
                }
                if (inKey && line.trim() !== '') {
                    base64Lines.push(line.trim());
                }
            }
            
            const publicKeyBase64 = base64Lines.join('');
            console.log('Extracted base64 key length:', publicKeyBase64.length);
            console.log('Extracted base64 key (first 50 chars):', publicKeyBase64.substring(0, 50));
            
            // Convert base64 to ArrayBuffer
            const keyData = this.base64ToArrayBuffer(publicKeyBase64);
            
            // Try to import as SPKI format first (most common for public keys)
            try {
                const key = await crypto.subtle.importKey(
                    'spki', // SubjectPublicKeyInfo format
                    keyData,
                    {
                        name: 'RSASSA-PKCS1-v1_5',
                        hash: { name: 'SHA-256' }
                    },
                    false, // not extractable
                    ['verify'] // only for verification
                );
                
                console.log('Successfully imported public key as SPKI format');
                return key;
            } catch (spkiError) {
                console.warn('Failed to import as SPKI, trying other formats:', spkiError);
                
                // If SPKI fails, the key might be in a different format
                // Try importing as raw RSA public key (less common)
                try {
                    // For raw RSA keys, we might need to wrap it in SPKI format
                    // This is a fallback - most keys should work with SPKI
                    const key = await crypto.subtle.importKey(
                        'spki',
                        keyData,
                        {
                            name: 'RSA-PSS',
                            hash: { name: 'SHA-256' }
                        },
                        false,
                        ['verify']
                    );
                    
                    console.log('Successfully imported public key as RSA-PSS format');
                    return key;
                } catch (rsaPssError) {
                    console.error('Failed to import key in any format');
                    throw spkiError; // Throw the original error
                }
            }
        } catch (error) {
            console.error('Failed to import public key:', error);
            console.error('Public key input (first 100 chars):', publicKeyInput.substring(0, 100));
            throw error;
        }
    },

    // Verify RSA signature
    async verifySignature(data, signature, publicKeyBase64) {
        try {
            console.log('Starting signature verification...');
            console.log('Data to verify (first 50 chars):', data.substring(0, 50));
            console.log('Signature (first 50 chars):', signature.substring(0, 50));
            
            // Import the public key
            const publicKey = await this.importPublicKey(publicKeyBase64);
            
            // Convert data to ArrayBuffer (ensure it's a string)
            const encoder = new TextEncoder();
            const dataBuffer = encoder.encode(String(data));
            
            // Clean and convert signature to ArrayBuffer
            const cleanSignature = signature.replace(/[\s\n\r]/g, '');
            const signatureBuffer = this.base64ToArrayBuffer(cleanSignature);
            
            console.log('Data buffer length:', dataBuffer.byteLength);
            console.log('Signature buffer length:', signatureBuffer.byteLength);
            
            // Verify the signature
            const isValid = await crypto.subtle.verify(
                {
                    name: 'RSASSA-PKCS1-v1_5'
                },
                publicKey,
                signatureBuffer,
                dataBuffer
            );
            
            console.log('Signature verification result:', isValid);
            return isValid;
        } catch (error) {
            console.error('Signature verification failed:', error);
            console.error('Error details:', {
                name: error.name,
                message: error.message,
                stack: error.stack
            });
            
            // Return false instead of throwing to allow graceful degradation
            return false;
        }
    },

    // Helper function to verify if a key is valid
    async testPublicKey(publicKeyBase64) {
        try {
            const key = await this.importPublicKey(publicKeyBase64);
            console.log('Public key test successful:', key);
            return true;
        } catch (error) {
            console.error('Public key test failed:', error);
            return false;
        }
    }
};