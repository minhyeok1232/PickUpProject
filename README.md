# 🎮 Pick Up Project(3D)
 
> Unity 기반 3D 뽑기 게임  

---

## 🛠 개발 환경
- **Engine** : Unity 6(6000.0.45f1)
- **IDE** : JetBrains Rider 2024.3

---

### UI Document
<details>
  <summary>🎇 자세히 </summary>
  
![image](https://github.com/user-attachments/assets/89d300f8-3fb6-4116-a065-5b9162461ea2)
![image](https://github.com/user-attachments/assets/50b8580e-3003-4844-b1f3-c84aa3527d56)
- UI Document (uxml, uss)

</details>

---

### Render Texture
<details>
  <summary>🎇 자세히 </summary>
  
![image](https://github.com/user-attachments/assets/fba66166-cfda-457e-a087-580975e340d0)
- 3D Particle -> 2D 처럼 보이게 (Camera 추가 설치)

</details>

---

### Tag와 Layer 분리

<details>
  <summary>🎇 자세히 </summary>
  
![image](https://github.com/user-attachments/assets/4c5f9902-605a-467e-969f-77f5d25ceba2)
- 순서
#### 1. Tag가 Prize인 선물을 대상으로 Collision 충돌 처리
####  2. 선물마다 Layer을 지정한 후 Layer(int)으로 선물마다 다르게 구현


</details>

---

### FBX 수정
<details>
  <summary>🎇 자세히 </summary>

#### URP
![image](https://github.com/user-attachments/assets/9cfc1543-1366-4b1d-b1e2-d7799b5c5618)
외부 FBX자료에서 사용하는 Material의 URP를 설정

#### Animation
![image](https://github.com/user-attachments/assets/9a00c796-499a-4851-b68a-04c95024db95)
각 집게마다 콜라이더 지정 후, 애니메이션 생성성

</details>

---
