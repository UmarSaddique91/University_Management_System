import React from 'react'
import { useState } from 'react'
import LoginImg from '../../assets/LoginImg.png'
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { Eye, EyeOff } from 'lucide-react';


const Login = () => {

  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await axios.post(
        "https://localhost:7272/api/Auth/login",
        { email: email.trim(), password: password }
      );

      localStorage.setItem("token", response.data.token);
      localStorage.setItem("roles", JSON.stringify(response.data.roles));
      localStorage.setItem("userName", response.data.name || (response.data.roles?.includes("SuperAdmin") ? "Super Admin" : "Admin"));

      const userRoles = response.data.roles || [];
      if (userRoles.includes("Teacher")) {
        navigate('/teacher/courses');
      } else if (userRoles.includes("Student")) {
        navigate('/student/courses');
      } else {
        navigate('/dashboard');
      }
    } catch (err) {
      const errorMessage =
        err.response?.data?.message ||
        "Invalid email or password. Please try again.";
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return (
       <div className="w-full min-h-screen bg-white flex">

        {/* Left SAECTION */}
        <div className="w-1/2 bg-white md:flex items-center justify-center">
        <img src={LoginImg} 
        alt="UMS Login Illustration" 
        className="object-cover h-full"/>
        </div>

        {/* Right Side Section */}

        <div className="w-1/2 bg-white flex flex-col items-center justify-center shadow-lg py-8">
        <div className="bg-white border border-gray-200 rounded-lg shadow-lg p-8 w-full max-w-md overflow-hidden">
            <h2 className="text-2xl font-bold mb-2">Welcome Back!</h2>
            <p className="text-slate-500">Sign in to access your UMS Dashboard</p>

            {/* Error Alert */}
            {error && (
              <div className="mt-4 p-3 bg-rose-50 border border-rose-200 text-rose-700 text-xs font-medium rounded-lg">
                {error}
              </div>
            )}

            {/* Form  */}
            <div>
              <form onSubmit={handleLogin} className="mt-6 space-y-4">

                {/* Email Address */}
                <div>
                  <label htmlFor="" className="font-semibold">Email Address</label>
                  <input type="email"
                  value={email}
                  onChange={(e)=>setEmail(e.target.value)}
                  placeholder="Enter your email"
                  required
                  className="w-full px-4 py-2 mt-2 border border-gray-800 hover:border-gray-300 focus rounded-md focus:outline-none  "
                  />
                </div>

                {/* Password  */}
                <div>
                  <label htmlFor="" className="font-semibold">Password</label>
                  <div className="relative">
                    <input 
                      type={showPassword ? "text" : "password"}
                      value={password}
                      onChange={(e)=>setPassword(e.target.value)}
                      placeholder="Enter your password"
                      required
                      className="w-full px-4 py-2 my-2 pr-10 border border-gray-800 hover:border-gray-300 focus rounded-md focus:outline-none"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700 cursor-pointer"
                    >
                      {showPassword ? <Eye size={18} /> : <EyeOff size={18} />}
                    </button>
                  </div>
                </div>

                <div className="flex justify-end items-center mt-4">
                  <span 
                    onClick={() => navigate("/forgotPassword")}
                    className="text-blue-500 hover:text-blue-700 cursor-pointer text-sm"
                  >
                    Forgot Password?
                  </span>
                </div>
              
              <button 
                type="submit" 
                disabled={loading}
                className="w-full bg-blue-500 hover:bg-blue-600 disabled:opacity-60 text-center py-3 cursor-pointer rounded-lg mt-6 text-white font-medium transition duration-150"
              >
                {loading ? "Signing In..." : "Sign In"}
              </button>
                
              </form>

              
            </div>

            {/* OR Divider */}
            <div className="flex items-center my-6">
              <div className="flex-1 border-t border-gray-200"></div>
              <span className="px-3 text-xs text-gray-400 font-medium">OR</span>
              <div className="flex-1 border-t border-gray-200"></div>
            </div>

            {/* Don't have an account */}
            <div 
            onClick={()=>navigate("signUp")}
            className="text-center">
              <p className="text-sm text-slate-500">
                Don't have an account?{' '}
                <span className="text-blue-500 font-semibold cursor-pointer hover:underline">
                  Sign Up
                </span>
              </p>
            </div>

        </div>

        {/* Copyright & Links outside sign-in card */}
        <div className="text-center mt-6 text-xs text-slate-400 space-y-1">
          <div className="flex justify-center items-center gap-2">
            <span className="hover:underline cursor-pointer">Privacy Policy</span>
            <span>|</span>
            <span className="hover:underline cursor-pointer">Terms of Service</span>
            <span>|</span>
            <span className="hover:underline cursor-pointer">Contact Support</span>
          </div>
          <p>© 2026 University Management System. All rights reserved.</p>
        </div>
        </div>

    </div>
  )
}

export default Login