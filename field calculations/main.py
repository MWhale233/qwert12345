import numpy as np
import matplotlib.pyplot as plt

# ---------------------------
# 参数设置
# ---------------------------
wavelength = 633e-9      # 波长，单位：米（此处为 633 nm）
k = 2 * np.pi / wavelength  # 波数
aperture_radius = 1e-3   # 圆孔半径，单位：米
grid_size = 5e-3         # 计算窗口尺寸，单位：米
N = 1024                 # 网格采样点数
dx = grid_size / N       # 网格间距

# ---------------------------
# 构建空间坐标和圆孔函数
# ---------------------------
x = np.linspace(-grid_size/2, grid_size/2, N)
y = np.linspace(-grid_size/2, grid_size/2, N)
X, Y = np.meshgrid(x, y)

# 定义圆形孔径（1 表示透过，0 表示阻挡）
aperture = np.zeros((N, N))
aperture[np.sqrt(X**2 + Y**2) <= aperture_radius] = 1

# ---------------------------
# 傅里叶域坐标（频率坐标）
# ---------------------------
fx = np.fft.fftfreq(N, d=dx)
fy = np.fft.fftfreq(N, d=dx)
FX, FY = np.meshgrid(fx, fy)

# 预计算初始场的傅里叶变换
U0 = np.fft.fft2(aperture)

# ---------------------------
# 设置传播距离，从 Fresnel 到 Fraunhofer 区域
# ---------------------------
z_min = 0.01  # 最小传播距离，单位：米
z_max = 1.0   # 最大传播距离，单位：米
num_z = 50    # z 方向上的采样数
z_values = np.linspace(z_min, z_max, num_z)

# 初始化用于存储每个 z 面场强的三维数组
field_intensity = np.zeros((num_z, N, N))

# ---------------------------
# 计算不同 z 平面上的衍射场（利用 Fresnel 传输函数）
# ---------------------------
for i, z in enumerate(z_values):
    # Fresnel传输函数：包含传播相位因子及二次相位因子
    H = np.exp(1j * k * z) * np.exp(-1j * np.pi * wavelength * z * (FX**2 + FY**2))
    # 利用傅里叶光学：域内乘以传输函数再反变换
    U_z = np.fft.ifft2(U0 * H)
    # 记录场强（取模平方）
    field_intensity[i, :, :] = np.abs(U_z)**2

# ---------------------------
# 可视化：展示不同传播距离处的衍射图样
# ---------------------------
# 例如，显示 z_min、z_mid 和 z_max 处的场强分布
fig, axes = plt.subplots(1, 3, figsize=(15, 5))
indices = [0, num_z//2, num_z - 1]
for ax, idx in zip(axes, indices):
    im = ax.imshow(field_intensity[idx, :, :],
                   extent=[x[0]*1e3, x[-1]*1e3, y[0]*1e3, y[-1]*1e3],
                   cmap='inferno')
    ax.set_title(f'z = {z_values[idx]:.3f} m')
    ax.set_xlabel('x (mm)')
    ax.set_ylabel('y (mm)')
    fig.colorbar(im, ax=ax)
plt.tight_layout()
plt.show()
