import React, { useState, useEffect } from 'react';
import LoginImg from '../../assets/LoginImg.png';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { UserPlus, Mail, Lock, Eye, EyeOff } from 'lucide-react';
import axios from 'axios';

const SignUp = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [invitationData, setInvitationData] = useState(null);

  // Validate invitation token on page load if present
  useEffect(() => {
    if (token) {
      axios
        .get(`https://localhost:7272/api/RegistrationInvitation/validate/${token}`)
        .then((res) => {
          setInvitationData(res.data);
          if (res.data?.recipientEmail) {
            setEmail(res.data.recipientEmail);
          }
        })
        .catch((err) => {
          setError(err.response?.data?.message || 'Invalid or expired invitation token');
        });
    }
  }, [token]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    try {
      await axios.post('https://localhost:7272/api/Auth/register', {
        email: email.trim(),
        password,
        token: token || '',
      });

      setSuccess('Account created successfully! Redirecting to login...');
      setTimeout(() => {
        navigate('/');
      }, 1800);
    } catch (err) {
      const errorMsg =
        err.response?.data?.message ||
        (Array.isArray(err.response?.data) && err.response?.data[0]?.description) ||
        'Registration failed. Please verify your details.';
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="w-full min-h-screen bg-white flex">
      {/* Left Side Section */}
      <div className="w-1/2 bg-white hidden md:flex items-center justify-center">
        <img
          src={LoginImg}
          alt="UMS Illustration"
          className="object-cover h-full"
        />
      </div>

      {/* Right Side Section */}
      <div className="w-full md:w-1/2 bg-[#f8faff] flex flex-col items-center justify-center p-8">
        <div className="bg-white border border-gray-100 rounded-3xl shadow-xl p-8 sm:p-10 w-full max-w-md">
          {/* UserPlus Icon Badge */}
          <div className="w-16 h-16 bg-blue-100/80 rounded-full flex items-center justify-center mx-auto mb-5">
            <UserPlus className="w-7 h-7 text-blue-600" />
          </div>

          {/* Heading & Subtitle */}
          <h2 className="text-2xl font-bold text-gray-900 text-center mb-2">
            Complete Registration
          </h2>
          <p className="text-sm text-gray-500 text-center leading-relaxed mb-6">
            {invitationData?.studentName
              ? `Welcome ${invitationData.studentName}! Set your password to activate your account.`
              : 'Create your password to access the University Management System.'}
          </p>

          {/* Feedback Alerts */}
          {error && (
            <div className="mb-4 p-3 bg-rose-50 border border-rose-200 text-rose-700 text-xs rounded-xl">
              {error}
            </div>
          )}

          {success && (
            <div className="mb-4 p-3 bg-emerald-50 border border-emerald-200 text-emerald-700 text-xs rounded-xl">
              {success}
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Email Address */}
            <div>
              <label htmlFor="email" className="block text-sm font-semibold text-gray-700 mb-1.5">
                Email Address
              </label>
              <div className="relative flex items-center">
                <Mail className="absolute left-3.5 w-4 h-4 text-gray-400 pointer-events-none" />
                <input
                  type="email"
                  id="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="Enter your email"
                  required
                  className="w-full pl-10 pr-4 py-2.5 bg-white border border-gray-200 rounded-xl text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label htmlFor="password" className="block text-sm font-semibold text-gray-700 mb-1.5">
                Password
              </label>
              <div className="relative flex items-center">
                <Lock className="absolute left-3.5 w-4 h-4 text-gray-400 pointer-events-none" />
                <input
                  type={showPassword ? 'text' : 'password'}
                  id="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Create password"
                  required
                  className="w-full pl-10 pr-10 py-2.5 bg-white border border-gray-200 rounded-xl text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3.5 text-gray-400 hover:text-gray-600 cursor-pointer"
                >
                  {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
                </button>
              </div>
            </div>

            {/* Password Requirements Box */}
            <div className="bg-[#f8faff] border border-gray-100 rounded-xl p-3.5 text-xs text-gray-500 space-y-1">
              <p className="font-semibold text-gray-700">Your password must:</p>
              <ul className="list-disc list-inside space-y-0.5 text-gray-500 pl-1">
                <li>Be at least 6 characters long</li>
                <li>Include at least one uppercase letter</li>
                <li>Include at least one lowercase letter</li>
                <li>Include at least one number</li>
                <li>Include at least one special character</li>
              </ul>
            </div>

            {/* Submit Button */}
            <div className="pt-2">
              <button
                type="submit"
                disabled={loading}
                className="w-full bg-blue-600 hover:bg-blue-700 disabled:opacity-60 text-white font-medium py-3 px-4 rounded-xl flex items-center justify-center cursor-pointer shadow-sm transition duration-150"
              >
                {loading ? 'Creating Account...' : 'Complete Registration'}
              </button>
            </div>
          </form>

          {/* Already have an account? Sign In */}
          <div className="text-center mt-6">
            <p className="text-sm text-gray-500">
              Already have an account?{' '}
              <span
                onClick={() => navigate('/')}
                className="text-blue-600 font-semibold cursor-pointer hover:underline"
              >
                Sign In
              </span>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default SignUp;
